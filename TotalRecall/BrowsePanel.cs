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
using System.Drawing.Drawing2D;
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

    /// <summary>Maximum rows a search returns; sourced from <see cref="AppSettings.SearchResultLimit"/>.</summary>
    [System.ComponentModel.Browsable(false)]
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public int ResultLimit { get; set; } = 200;

    // Default time scope on first load. With a large DB we bound the initial query to a
    // recent window so the Browse pane never scans the whole database on open.
    private const string DefaultRange = "Last 7 days";

    // Monotonic search id. Each search bumps it; a completed search only updates the UI
    // if it's still the latest, which serialises overlapping requests.
    private int searchGen;

    // Result sort state. Column indices: 0=When, 1=Title, 2=Snippet, 3=App.
    // Default matches the DB query order: newest first (When, descending).
    private static readonly string[] colBase = { "When", "Title", "Snippet", "App" };
    private int sortColumn;
    private bool sortAscending;

    // Snapshot of the app-name filter the combo currently shows. Cheap
    // string-set compare lets us skip rebuilding the combo on every tick
    // (which flickers, drops the dropdown, and loses the typing caret).
    private HashSet<string> lastAppFilterSet = new(StringComparer.Ordinal);
    private DateTime lastAppFilterLoadUtc = DateTime.MinValue;

    // Coalesce refresh requests: TickAsync fires every N seconds, but if a
    // refresh is already running we'll just mark dirty and the in-flight
    // call will repaint at the end. Avoids overlapping DB queries.
    private bool refreshInFlight;

    public BrowsePanel()
    {
        InitializeComponent();
        BuildCollapseIcons();
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
    /// Generates the arrow-bar glyphs for the snippet collapse / restore buttons. We draw
    /// them in code (instead of shipping image assets) so they stay crisp and theme-coloured.
    /// The shapes mirror Bootstrap's <c>arrow-bar-right</c> (collapse the panel rightward)
    /// and <c>arrow-bar-left</c> (bring it back).
    /// </summary>
    private void BuildCollapseIcons()
    {
        collapseTextBtn.Image = MakeBarArrow(pointRight: true);
        collapseTextBtn.ImageAlign = ContentAlignment.MiddleCenter;
        toggleSnippetBtn.Image = MakeBarArrow(pointRight: false);
        toggleSnippetBtn.ImageAlign = ContentAlignment.MiddleCenter;
    }

    private static Bitmap MakeBarArrow(bool pointRight)
    {
        var bmp = new Bitmap(16, 16);
        bmp.SetResolution(96, 96);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        using var pen = new Pen(Color.FromArgb(28, 28, 30), 1.6f)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round,
            LineJoin = LineJoin.Round,
        };
        const int mid = 8;
        if (pointRight)
        {
            g.DrawLine(pen, 13, 3, 13, 13);   // bar on the right edge
            g.DrawLine(pen, 3, mid, 11, mid);  // shaft
            g.DrawLine(pen, 11, mid, 8, mid - 3);
            g.DrawLine(pen, 11, mid, 8, mid + 3);
        }
        else
        {
            g.DrawLine(pen, 3, 3, 3, 13);     // bar on the left edge
            g.DrawLine(pen, 13, mid, 5, mid);  // shaft
            g.DrawLine(pen, 5, mid, 8, mid - 3);
            g.DrawLine(pen, 5, mid, 8, mid + 3);
        }
        return bmp;
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
            if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; _ = DoSearchAsync(userInitiated: true); }
        };
        appCombo.KeyDown += (s, e) =>
        {
            if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; _ = DoSearchAsync(userInitiated: true); }
        };
        searchBtn.Click += async (_, _) => await DoSearchAsync(userInitiated: true);

        // Picking a time range *is* the action — no separate Filter button. "Custom…"
        // reveals the two date pickers; everything else searches immediately.
        rangeCombo.SelectedIndexChanged += async (_, _) =>
        {
            UpdateCustomRangeVisibility();
            await DoSearchAsync(userInitiated: true);
        };
        fromDate.ValueChanged += async (_, _) => { if (IsCustomRange) await DoSearchAsync(userInitiated: true); };
        toDate.ValueChanged += async (_, _) => { if (IsCustomRange) await DoSearchAsync(userInitiated: true); };

        refreshBtn.Click += async (_, _) =>
        {
            searchBox.Text = "";
            if (appCombo.Items.Count > 0)
            {
                appCombo.SelectedIndex = 0;
                appCombo.Text = "(all apps)";
            }
            rangeCombo.SelectedItem = DefaultRange;
            UpdateCustomRangeVisibility();
            await DoSearchAsync(userInitiated: true);
        };
        results.SelectedIndexChanged += async (_, _) => await UpdatePreviewAsync();
        results.ColumnClick += OnColumnClick;

        copyTextBtn.Click += (_, _) => CopyTextToClipboard();
        collapseTextBtn.Click += (_, _) => CollapseTextPane();
        toggleSnippetBtn.Click += (_, _) => RestoreTextPane();

        fromDate.Value = DateTime.Today.AddDays(-7);
        toDate.Value = DateTime.Today;
        rangeCombo.SelectedItem = DefaultRange;
        UpdateCustomRangeVisibility();
        UpdateSortIndicators();
    }

    private bool IsCustomRange => (rangeCombo.SelectedItem as string) == "Custom…";

    private void UpdateCustomRangeVisibility()
    {
        var custom = IsCustomRange;
        fromDate.Visible = custom;
        customToLbl.Visible = custom;
        toDate.Visible = custom;
    }

    /// <summary>
    /// Resolves the selected preset into an inclusive [from, to] window, or (null, null) for
    /// "Any time". Presets are anchored to "now" so they always mean what the label says.
    /// </summary>
    private (DateTimeOffset? from, DateTimeOffset? to) ResolveTimeRange()
    {
        var now = DateTimeOffset.Now;
        var today = DateTime.Today;
        if (IsCustomRange)
        {
            // Guard against an inverted range (user sets the "to" date before the "from"
            // date) — swap so the window is always [earlier, later] and still returns rows
            // instead of silently showing nothing.
            var a = fromDate.Value.Date;
            var b = toDate.Value.Date;
            var lo = a <= b ? a : b;
            var hi = a <= b ? b : a;
            return (new DateTimeOffset(lo), new DateTimeOffset(hi.AddDays(1).AddSeconds(-1)));
        }
        return (rangeCombo.SelectedItem as string) switch
        {
            "Today"         => (new DateTimeOffset(today), now),
            "Last 24 hours" => (now.AddHours(-24), now),
            "Last 7 days"   => (new DateTimeOffset(today.AddDays(-6)), now),
            "Last 30 days"  => (new DateTimeOffset(today.AddDays(-29)), now),
            "This year"     => (new DateTimeOffset(new DateTime(today.Year, 1, 1)), now),
            _               => (null, null), // "Any time"
        };
    }

    // --- Column sorting -------------------------------------------------

    private void OnColumnClick(object? sender, ColumnClickEventArgs e)
    {
        if (e.Column == sortColumn)
        {
            sortAscending = !sortAscending;
        }
        else
        {
            sortColumn = e.Column;
            // Time defaults to newest-first; text columns default to A→Z.
            sortAscending = e.Column != 0;
        }
        UpdateSortIndicators();
        PopulateResults();
    }

    private void UpdateSortIndicators()
    {
        for (int i = 0; i < results.Columns.Count && i < colBase.Length; i++)
        {
            var arrow = i == sortColumn ? (sortAscending ? "  ▲" : "  ▼") : "";
            results.Columns[i].Text = colBase[i] + arrow;
        }
    }

    private void SortHits()
    {
        Comparison<SearchHit> cmp = sortColumn switch
        {
            1 => (a, b) => string.Compare(a.Title ?? "", b.Title ?? "", StringComparison.OrdinalIgnoreCase),
            2 => (a, b) => string.Compare(a.Snippet ?? "", b.Snippet ?? "", StringComparison.OrdinalIgnoreCase),
            3 => (a, b) => string.Compare(a.AppName ?? "", b.AppName ?? "", StringComparison.OrdinalIgnoreCase),
            _ => CompareWhen,
        };
        hits.Sort(cmp);
        if (!sortAscending) hits.Reverse();
    }

    private static int CompareWhen(SearchHit a, SearchHit b)
    {
        DateTimeOffset.TryParse(a.Timestamp, out var da);
        DateTimeOffset.TryParse(b.Timestamp, out var dbb);
        return da.CompareTo(dbb);
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
        ReloadAppFilter(force: false);
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
        ReloadAppFilter(force: true);
        _ = DoSearchAsync(userInitiated: true);
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
    private void ReloadAppFilter(bool force)
    {
        if (db == null) return;
        // The distinct-app query scans the whole app_name index. On a multi-GB DB that's
        // wasteful to run on every capture tick, so throttle auto-refreshes to once a
        // minute. Explicit refreshes (force) always re-query.
        if (!force && appCombo.Items.Count > 0
            && (DateTime.UtcNow - lastAppFilterLoadUtc) < TimeSpan.FromMinutes(1))
            return;
        lastAppFilterLoadUtc = DateTime.UtcNow;
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

    private async Task DoSearchAsync(bool userInitiated = false)
    {
        if (db == null) { ClearUi(); return; }
        var query = searchBox.Text.Trim();
        var appText = (appCombo.Text ?? "").Trim();
        string? app = (appText.Length == 0 || appText == "(all apps)") ? null : appText;
        var (from, to) = ResolveTimeRange();
        int limit = ResultLimit;

        // Single-flight + generation guard. Each call bumps the generation; when a query
        // completes we only touch the UI if no newer query has started since. This stops
        // overlapping user clicks (e.g. Filter, Filter again) from fighting over the panel
        // and looping the "Loading…" state forever.
        var myGen = ++searchGen;
        refreshInFlight = true;
        if (userInitiated) SetBusy(true);
        else searchBtn.Enabled = false;
        if (userInitiated || hits.Count == 0) resultsCountLbl.Text = "Loading…";
        try
        {
            var newHits = await Task.Run(() => db.Search(query, app, from, to, limit));
            if (myGen != searchGen) return; // a newer search superseded us — let it win
            this.hits = newHits;
            PopulateResults();
            resultsCountLbl.Text = $"{newHits.Count} result(s)";
        }
        catch (Exception ex)
        {
            if (myGen != searchGen) return;
            resultsCountLbl.Text = "Error: " + ex.Message;
            hits.Clear();
            results.Items.Clear();
        }
        finally
        {
            // Only the latest query restores the controls / coalesces; a superseded one
            // bows out silently so it can't re-enable controls the newer query just disabled.
            if (myGen == searchGen)
            {
                if (userInitiated) SetBusy(false);
                else searchBtn.Enabled = true;
                refreshInFlight = false;
                if (dirty) RefreshIfNeeded();
            }
        }
    }

    /// <summary>
    /// Marks the panel busy during a user-initiated query: disables the search/filter
    /// controls and clears the result + preview panes behind a "Loading…" label so it's
    /// obvious the database is being queried (no spinning wait-cursor).
    /// </summary>
    private void SetBusy(bool busy)
    {
        searchBox.Enabled = !busy;
        appCombo.Enabled = !busy;
        rangeCombo.Enabled = !busy;
        fromDate.Enabled = !busy;
        toDate.Enabled = !busy;
        searchBtn.Enabled = !busy;
        refreshBtn.Enabled = !busy;
        if (busy)
        {
            results.Items.Clear();
            ClearPreview();
            resultsCountLbl.Text = "Loading…";
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
        SortHits();

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
