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
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TotalRecall;

public partial class BrowsePanel : UserControl
{
    private Database? db;
    private bool dirty = true;
    private List<SearchHit> hits = new();

    public BrowsePanel()
    {
        InitializeComponent();
        WireEvents();
    }

    private void WireEvents()
    {
        searchBox.KeyDown += (s, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                _ = DoSearchAsync();
            }
        };
        appCombo.KeyDown += (s, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                _ = DoSearchAsync();
            }
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

        fromDate.Value = DateTime.Today.AddDays(-7);
        toDate.Value = DateTime.Today.AddDays(1);
    }

    public void AttachDatabase(Database? db)
    {
        this.db = db;
        dirty = true;
        if (db == null) ClearUi();
    }

    public void InvalidateData() => dirty = true;

    public void RefreshIfNeeded()
    {
        if (!dirty || db == null) return;
        dirty = false;
        ReloadAppFilter();
        _ = DoSearchAsync();
    }

    private void ReloadAppFilter()
    {
        if (db == null) return;
        try
        {
            appCombo.BeginUpdate();
            appCombo.Items.Clear();
            appCombo.Items.Add("(all apps)");
            foreach (var a in db.GetDistinctAppNames()) appCombo.Items.Add(a);
            appCombo.SelectedIndex = 0;
            appCombo.Text = "(all apps)";
        }
        finally { appCombo.EndUpdate(); }
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

        searchBtn.Enabled = false;
        resultsCountLbl.Text = "Searching…";
        try
        {
            var hits = await Task.Run(() => db.Search(query, app, from, to, limit));
            this.hits = hits;
            PopulateResults();
            resultsCountLbl.Text = $"{hits.Count} result(s)";
        }
        catch (Exception ex)
        {
            resultsCountLbl.Text = "Error: " + ex.Message;
            hits.Clear();
            results.Items.Clear();
        }
        finally { searchBtn.Enabled = true; }
    }

    private void PopulateResults()
    {
        results.BeginUpdate();
        results.Items.Clear();
        foreach (var h in hits)
        {
            var when = h.Timestamp;
            if (DateTimeOffset.TryParse(h.Timestamp, out var dto))
                when = dto.ToLocalTime().ToString("MM-dd HH:mm:ss");
            var snippet = (h.Snippet ?? "").Replace("\r", " ").Replace("\n", " ");
            var item = new ListViewItem(new[]
            {
                when,
                h.AppName ?? "",
                h.Title ?? "",
                snippet,
                (h.ImageBytes / 1024).ToString(),
                h.TextLength.ToString(),
            });
            item.Tag = h;
            results.Items.Add(item);
        }
        results.EndUpdate();
        if (results.Items.Count > 0) results.Items[0].Selected = true;
        else ClearPreview();
    }

    private async Task UpdatePreviewAsync()
    {
        if (db == null || results.SelectedItems.Count == 0) { ClearPreview(); return; }
        var hit = (SearchHit)results.SelectedItems[0].Tag!;
        try
        {
            var detail = await Task.Run(() => db.GetWindowDetail(hit.WindowId, includeImage: true));
            if (detail == null) { ClearPreview(); return; }

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

    private void searchBtn_Click(object sender, EventArgs e)
    {

    }
}
