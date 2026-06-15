// TotalRecall — a local screen-activity indexer.
// Copyright (C) 2026 Ilya Fainberg.
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TotalRecall;

/// <summary>
/// The main browsing surface: search, results list, screenshot preview (zoom + pan),
/// and full OCR text in a collapsible/resizable side panel.
/// </summary>
/// <remarks>
/// <para>Layout (left → right, designed in <c>BrowsePanel.Designer.cs</c>):</para>
/// <list type="number">
///   <item><b>Results panel</b> — ListView with When | Title | Snippet | App columns,
///     fed by <see cref="Database.Search"/>. Filters: free-text search + app dropdown +
///     a sliding-window time filter (today, 24h, 7d, all).</item>
///   <item><b>Preview panel</b> — <see cref="ZoomablePicturePanel"/> with a zoom slider
///     and the hand-cursor pan implementation. See <see cref="sliderToPercent"/> for
///     the zoom step table.</item>
///   <item><b>Text panel</b> — full OCR text for the selected row in a multiline
///     TextBox. Collapsible via the header chevron button; the splitter distance is
///     remembered in <see cref="lastInnerSplitterDistance"/> so re-expanding restores
///     the user's chosen width.</item>
/// </list>
/// <para><b>Keyboard shortcuts</b> are routed through <see cref="TryHandleShortcut"/>
/// from <see cref="MainForm.ProcessCmdKey"/>. That keeps the single source of truth
/// for shortcut routing in <see cref="MainForm"/> and avoids double-invokes.</para>
/// </remarks>
public partial class BrowsePanel : UserControl
{
    private Database? db;
    private bool dirty = true;
    private List<SearchHit> hits = new();
    private WindowDetail? currentDetail;
    private int? lastInnerSplitterDistance;

    // Snapshot of the app-name filter the combo currently shows. Cheap
    // string-set compare lets us skip rebuilding the combo on every tick
    // (which flickers, drops the dropdown, and loses the typing caret).
    private HashSet<string> lastAppFilterSet = new(StringComparer.Ordinal);

    // Coalesce refresh requests: TickAsync fires every N seconds, but if a
    // refresh is already running we'll just mark dirty and the in-flight
    // call will repaint at the end. Avoids overlapping DB queries.
    private bool refreshInFlight;

    public BrowsePanel()
    {
        InitializeComponent();
        WireEvents();
        WireZoom();
        WireContextMenus();
        EnableDoubleBufferingHacks();
        VisibleChanged += (_, _) =>
        {
            // User came back to a hidden Browse pane (e.g. restored from tray) —
            // surface any data the tick produced while we weren't looking.
            if (Visible) RefreshIfNeeded();
        };
    }

    /// <summary>
    /// Turns on double-buffering for the inner ListView via reflection (the
    /// <see cref="Control.DoubleBuffered"/> property is protected on the base
    /// class, so the Designer can't set it). Without this, every auto-tick
    /// repaint of the results list flashes white.
    /// </summary>
    private void EnableDoubleBufferingHacks()
    {
        try
        {
            var prop = typeof(Control).GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            prop?.SetValue(results, true);
        }
        catch { /* best-effort UX polish, never block the app */ }
    }

    private void WireEvents()
    {
        searchBox.KeyDown += (s, e) =>
        {
            if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; _ = DoSearchAsync(); }
        };
        appCombo.KeyDown += (s, e) =>
        {
            if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; _ = DoSearchAsync(); }
        };
        searchBtn.Click += async (_, _) => await DoSearchAsync();
        refreshBtn.Click += async (_, _) =>
        {
            searchBox.Text = "";
            if (appCombo.Items.Count > 0)
            {
                appCombo.SelectedIndex = 0;
                appCombo.Text = "(all apps)";
            }
            useDateRange.Checked = false;
            await DoSearchAsync();
        };
        results.SelectedIndexChanged += async (_, _) => await UpdatePreviewAsync();

        copyTextBtn.Click += (_, _) => CopyTextToClipboard();
        collapseTextBtn.Click += (_, _) => CollapseTextPane();
        toggleSnippetBtn.Click += (_, _) => RestoreTextPane();

        fromDate.Value = DateTime.Today.AddDays(-7);
        toDate.Value = DateTime.Today.AddDays(1);
    }

    private void WireZoom()
    {
        zoomBar.ValueChanged += (_, _) => ApplyZoomFromSlider();
        preview.ZoomChanged += (_, _) => SyncSliderFromZoom();
        ApplyZoomFromSlider();
    }

    private void WireContextMenus()
    {
        previewCtx.Items.Add(new ToolStripMenuItem("Open in default viewer", null, (_, _) => OpenPreviewExternal()));
        previewCtx.Items.Add(new ToolStripMenuItem("Save image as…", null, (_, _) => SavePreviewAs()));
        previewCtx.Items.Add(new ToolStripSeparator());
        previewCtx.Items.Add(new ToolStripMenuItem("Fit to window (Ctrl+0)", null, (_, _) => preview.FitToWindow()));
        previewCtx.Items.Add(new ToolStripMenuItem("100% (Ctrl+1)", null, (_, _) => preview.OneToOne()));

        textCtx.Items.Add(new ToolStripMenuItem("Copy", null, (_, _) => CopyTextToClipboard()));
        textCtx.Items.Add(new ToolStripMenuItem("Select all", null, (_, _) => { previewText.SelectAll(); previewText.Focus(); }));
    }

    public void AttachDatabase(Database? db)
    {
        this.db = db;
        dirty = true;
        if (db == null) ClearUi();
    }

    /// <summary>
    /// Marks the visible data as stale. Safe to call from any thread; the
    /// next <see cref="RefreshIfNeeded"/> picks up the flag. The capture
    /// loop calls this every tick.
    /// </summary>
    public void InvalidateData() => dirty = true;

    /// <summary>
    /// Re-runs the current query if the panel is visible, the DB is open,
    /// data is dirty, and the user isn't actively editing the search box.
    /// </summary>
    /// <remarks>
    /// Intentionally conservative: we skip the refresh when the user is
    /// typing in <see cref="searchBox"/> so a tick mid-keystroke doesn't
    /// rip the dropdown shut or steal the caret. The dirty flag stays set
    /// so the next valid refresh picks up.
    /// </remarks>
    public void RefreshIfNeeded()
    {
        if (db == null || !dirty) return;
        if (!Visible || !IsHandleCreated) return;
        if (refreshInFlight) return;
        if (searchBox.Focused && !string.IsNullOrEmpty(searchBox.Text)) return;
        if (appCombo.Focused && appCombo.DroppedDown) return;

        dirty = false;
        ReloadAppFilter();
        _ = DoSearchAsync();
    }

    /// <summary>
    /// F5 / hamburger-Refresh: always re-run the search, even when not
    /// dirty and even when the search box is focused. The user asked
    /// explicitly so we honour it.
    /// </summary>
    public void ForceRefresh()
    {
        if (db == null) return;
        dirty = false;
        ReloadAppFilter();
        _ = DoSearchAsync();
    }

    public void FocusSearch()
    {
        searchBox.Focus();
        searchBox.SelectAll();
    }

    /// <summary>
    /// Rebuilds the app-filter combo only when the distinct app set in the
    /// DB actually changed since last time. This stops every tick from
    /// blowing away the user's current selection / dropdown position.
    /// </summary>
    private void ReloadAppFilter()
    {
        if (db == null) return;
        try
        {
            var current = db.GetDistinctAppNames().ToList();
            var currentSet = new HashSet<string>(current, StringComparer.Ordinal);

            // Nothing changed (and we already populated at least once) → no-op.
            if (appCombo.Items.Count > 0 && currentSet.SetEquals(lastAppFilterSet))
                return;

            // Preserve whatever the user had selected if it's still in the list.
            var previousText = appCombo.Text;

            appCombo.BeginUpdate();
            try
            {
                appCombo.Items.Clear();
                appCombo.Items.Add("(all apps)");
                foreach (var a in current) appCombo.Items.Add(a);
                if (!string.IsNullOrEmpty(previousText) && currentSet.Contains(previousText))
                {
                    appCombo.Text = previousText;
                }
                else
                {
                    appCombo.SelectedIndex = 0;
                    appCombo.Text = "(all apps)";
                }
            }
            finally { appCombo.EndUpdate(); }

            lastAppFilterSet = currentSet;
        }
        catch { /* combo rebuild is cosmetic — never break the panel */ }
    }

    private async Task DoSearchAsync()
    {
        if (db == null) { ClearUi(); return; }
        var query = searchBox.Text.Trim();
        var appText = (appCombo.Text ?? "").Trim();
        string? app = (appText.Length == 0 || appText == "(all apps)") ? null : appText;
        DateTimeOffset? from = null, to = null;
        if (useDateRange.Checked)
        {
            from = new DateTimeOffset(fromDate.Value.Date);
            to = new DateTimeOffset(toDate.Value.Date.AddDays(1).AddSeconds(-1));
        }
        int limit = (int)limitNud.Value;

        refreshInFlight = true;
        searchBtn.Enabled = false;
        // Don't flash a "Searching…" message on every auto-tick — only when
        // the count would actually be empty. Auto-refreshes should feel
        // invisible; user-initiated searches will still see it via the
        // initial empty state.
        var showProgress = hits.Count == 0;
        if (showProgress) resultsCountLbl.Text = "Searching…";
        try
        {
            var newHits = await Task.Run(() => db.Search(query, app, from, to, limit));
            this.hits = newHits;
            PopulateResults();
            resultsCountLbl.Text = $"{newHits.Count} result(s)";
        }
        catch (Exception ex)
        {
            resultsCountLbl.Text = "Error: " + ex.Message;
            hits.Clear();
            results.Items.Clear();
        }
        finally
        {
            searchBtn.Enabled = true;
            refreshInFlight = false;
            // Tick may have marked us dirty again while we were querying — coalesce.
            if (dirty) RefreshIfNeeded();
        }
    }

    /// <summary>
    /// Renders <see cref="hits"/> into the results ListView. Preserves the
    /// previously-selected row (by WindowId) and the user's scroll position
    /// so auto-refreshes never yank the user out of whatever they were
    /// inspecting.
    /// </summary>
    private void PopulateResults()
    {
        // Snapshot current selection + scroll so we can restore them.
        long? previouslySelectedWindowId = results.SelectedItems.Count > 0
            ? ((SearchHit)results.SelectedItems[0].Tag!).WindowId
            : null;
        int previousTopIndex = -1;
        try { previousTopIndex = results.TopItem?.Index ?? -1; } catch { }

        results.BeginUpdate();
        try
        {
            results.Items.Clear();
            ListViewItem? restoreSelection = null;
            foreach (var h in hits)
            {
                var when = h.Timestamp;
                if (DateTimeOffset.TryParse(h.Timestamp, out var dto))
                    when = dto.ToLocalTime().ToString("MM-dd HH:mm:ss");
                var snippet = (h.Snippet ?? "").Replace("\r", " ").Replace("\n", " ");
                var item = new ListViewItem(new[]
                {
                    when,
                    h.Title ?? "",
                    snippet,
                    h.AppName ?? "",
                })
                { Tag = h };
                results.Items.Add(item);
                if (previouslySelectedWindowId is long pid && h.WindowId == pid)
                    restoreSelection = item;
            }

            if (restoreSelection != null)
            {
                restoreSelection.Selected = true;
                restoreSelection.EnsureVisible();
            }
            else if (results.Items.Count > 0)
            {
                results.Items[0].Selected = true;
            }
            else
            {
                ClearPreview();
            }

            // Best-effort restore of scroll position (only when nothing was selected
            // — if we re-selected the user's row it's already visible).
            if (restoreSelection == null && previousTopIndex >= 0 && previousTopIndex < results.Items.Count)
            {
                try { results.TopItem = results.Items[previousTopIndex]; } catch { }
            }
        }
        finally { results.EndUpdate(); }
    }

    private async Task UpdatePreviewAsync()
    {
        if (db == null || results.SelectedItems.Count == 0) { ClearPreview(); return; }
        var hit = (SearchHit)results.SelectedItems[0].Tag!;
        try
        {
            var detail = await Task.Run(() => db.GetWindowDetail(hit.WindowId, includeImage: true));
            if (detail == null) { ClearPreview(); return; }
            currentDetail = detail;

            previewTitleLbl.Text = $"{detail.AppName}   ·   {detail.Title}";
            previewMetaLbl.Text =
                $"{detail.Timestamp}    pid {detail.ProcessId} ({detail.ProcessName})    " +
                $"{detail.BoundsW}×{detail.BoundsH}    " +
                $"{(detail.ImageBytes > 0 ? MainForm.FormatBytes(detail.ImageBytes) : "no image")}" +
                (detail.OcrError != null ? "    ⚠ " + detail.OcrError : "");

            var oldImage = preview.Image;
            if (detail.JpegBytes != null && detail.JpegBytes.Length > 0)
            {
                using var ms = new MemoryStream(detail.JpegBytes);
                preview.Image = Image.FromStream(ms);
            }
            else
            {
                preview.Image = null;
            }
            oldImage?.Dispose();

            previewText.Text = detail.Text ?? "";
        }
        catch (Exception ex)
        {
            previewText.Text = "Error loading detail: " + ex.Message;
        }
    }

    private void ClearPreview()
    {
        currentDetail = null;
        previewTitleLbl.Text = "Select a result to preview";
        previewMetaLbl.Text = "";
        previewText.Text = "";
        var old = preview.Image; preview.Image = null; old?.Dispose();
    }

    private void ClearUi()
    {
        results.Items.Clear();
        appCombo.Items.Clear();
        resultsCountLbl.Text = "Database not open. Open Settings or start a capture.";
        ClearPreview();
    }

    // --- Zoom plumbing --------------------------------------------------

    // Maps zoomBar.Value (0..6) to ZoomablePicturePanel.ZoomPercent.
    // 0 = Fit, 1 = 25, 2 = 50, 3 = 75, 4 = 100, 5 = 150, 6 = 200.
    private static readonly int[] sliderToPercent = { 0, 25, 50, 75, 100, 150, 200 };

    private void ApplyZoomFromSlider()
    {
        var v = Math.Clamp(zoomBar.Value, zoomBar.Minimum, zoomBar.Maximum);
        var pct = sliderToPercent[v];
        if (pct != preview.ZoomPercent) preview.ZoomPercent = pct;
        zoomValueLbl.Text = pct == 0 ? "Fit" : $"{pct}%";
    }

    private void SyncSliderFromZoom()
    {
        int idx = Array.IndexOf(sliderToPercent, preview.ZoomPercent);
        if (idx < 0) idx = 0;
        if (zoomBar.Value != idx) zoomBar.Value = idx;
        zoomValueLbl.Text = preview.ZoomPercent == 0 ? "Fit" : $"{preview.ZoomPercent}%";
    }

    public bool TryHandleShortcut(Keys keyData)
    {
        // Ctrl+0 → Fit, Ctrl+1 → 100%, Ctrl+Plus / Ctrl+OemPlus → in, Ctrl+Minus / Ctrl+OemMinus → out.
        // F5 → refresh, Ctrl+F → focus search.
        if ((keyData & Keys.Control) == Keys.Control)
        {
            var key = keyData & Keys.KeyCode;
            switch (key)
            {
                case Keys.D0:
                case Keys.NumPad0:
                    preview.FitToWindow();
                    return true;
                case Keys.D1:
                case Keys.NumPad1:
                    preview.OneToOne();
                    return true;
                case Keys.Add:
                case Keys.Oemplus:
                    preview.ZoomIn();
                    return true;
                case Keys.Subtract:
                case Keys.OemMinus:
                    preview.ZoomOut();
                    return true;
                case Keys.F:
                    FocusSearch();
                    return true;
            }
        }
        if (keyData == Keys.F5)
        {
            ForceRefresh();
            return true;
        }
        return false;
    }

    // --- Collapse plumbing ----------------------------------------------

    private void CollapseTextPane()
    {
        if (innerSplit.Panel2Collapsed) return;
        lastInnerSplitterDistance = innerSplit.SplitterDistance;
        innerSplit.Panel2Collapsed = true;
        toggleSnippetBtn.Visible = true;
    }

    private void RestoreTextPane()
    {
        if (!innerSplit.Panel2Collapsed) return;
        innerSplit.Panel2Collapsed = false;
        if (lastInnerSplitterDistance is int d && d > innerSplit.Panel1MinSize)
        {
            try { innerSplit.SplitterDistance = d; } catch { /* layout race */ }
        }
        toggleSnippetBtn.Visible = false;
    }

    // --- Text / image actions -------------------------------------------

    private void CopyTextToClipboard()
    {
        var txt = previewText.Text;
        if (string.IsNullOrEmpty(txt)) return;
        try { Clipboard.SetText(txt); } catch { /* clipboard can be temporarily locked */ }
    }

    private void OpenPreviewExternal()
    {
        if (currentDetail?.JpegBytes is not { Length: > 0 } bytes) return;
        try
        {
            var path = Path.Combine(Path.GetTempPath(), $"TotalRecall-preview-{currentDetail.WindowId}.jpg");
            File.WriteAllBytes(path, bytes);
            Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true });
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "Could not open image: " + ex.Message, "TotalRecall",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void SavePreviewAs()
    {
        if (currentDetail?.JpegBytes is not { Length: > 0 } bytes) return;
        using var dlg = new SaveFileDialog
        {
            Title = "Save screenshot",
            Filter = "JPEG image (*.jpg)|*.jpg",
            FileName = $"TotalRecall-{currentDetail.WindowId}.jpg",
        };
        if (dlg.ShowDialog(this) == DialogResult.OK)
        {
            try { File.WriteAllBytes(dlg.FileName, bytes); }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Could not save image: " + ex.Message, "TotalRecall",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
