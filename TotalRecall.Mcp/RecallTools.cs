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
using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;
using TotalRecall;

namespace TotalRecall.Mcp;

[McpServerToolType]
public sealed class RecallTools
{
    private readonly Database db;

    public RecallTools(Database db) { this.db = db; }

    [McpServerTool(Name = "search_recall")]
    [Description("""
        Full-text search the TotalRecall database of screen activity.
        The 'query' is FTS5 syntax over (title, app_name, process_name, ocr_text).
        Examples:
          - 'invoice'                     simple term
          - '"q3 plan"'                   exact phrase
          - 'invoice AND outlook'         boolean
          - 'invoic*'                     prefix
        Optional filters narrow further. Returns up to 'limit' window rows
        (default 25, max 200), newest first.
        """)]
    public object SearchRecall(
        [Description("FTS5 query string. Leave empty to browse-by-filter only.")] string? query = null,
        [Description("Optional exact app_name to filter by (e.g. 'Microsoft Outlook').")] string? app = null,
        [Description("Optional ISO 8601 lower-bound timestamp (inclusive).")] string? from = null,
        [Description("Optional ISO 8601 upper-bound timestamp (inclusive).")] string? to = null,
        [Description("Max number of results (1..200).")] int limit = 25)
    {
        limit = Math.Clamp(limit, 1, 200);
        DateTimeOffset? f = ParseTs(from);
        DateTimeOffset? t = ParseTs(to);
        var hits = db.Search(query, app, f, t, limit);
        return new { count = hits.Count, results = hits };
    }

    [McpServerTool(Name = "get_window")]
    [Description("Get full details for a single window row (including full OCR text). Set include_image to also return the JPEG bytes as base64.")]
    public object GetWindow(
        [Description("Window row id returned by search_recall.")] long window_id,
        [Description("If true, include the stored JPEG as base64 in the 'image_base64' field.")] bool include_image = false)
    {
        var d = db.GetWindowDetail(window_id, include_image);
        if (d == null) return new { error = "not_found", window_id };

        return new
        {
            window_id = d.WindowId,
            snapshot_id = d.SnapshotId,
            timestamp = d.Timestamp,
            user = d.User,
            machine = d.Machine,
            app_name = d.AppName,
            title = d.Title,
            process_name = d.ProcessName,
            process_id = d.ProcessId,
            executable_path = d.ExecutablePath,
            is_foreground = d.IsForeground,
            bounds = new { x = d.BoundsX, y = d.BoundsY, w = d.BoundsW, h = d.BoundsH },
            ocr_text = d.Text,
            ocr_error = d.OcrError,
            ocr_duration_ms = d.OcrDurationMs,
            image_bytes = d.ImageBytes,
            image_base64 = include_image && d.JpegBytes != null ? Convert.ToBase64String(d.JpegBytes) : null,
            image_mime = include_image && d.JpegBytes != null ? "image/jpeg" : null,
        };
    }

    [McpServerTool(Name = "list_snapshots")]
    [Description("List capture snapshots in reverse-chronological order. A snapshot = one tick of the capture loop and groups multiple window rows.")]
    public object ListSnapshots(
        [Description("Optional ISO 8601 lower-bound timestamp.")] string? from = null,
        [Description("Optional ISO 8601 upper-bound timestamp.")] string? to = null,
        [Description("Max number of snapshots (1..500).")] int limit = 50)
    {
        limit = Math.Clamp(limit, 1, 500);
        var list = db.ListSnapshots(ParseTs(from), ParseTs(to), limit);
        return new { count = list.Count, snapshots = list };
    }

    [McpServerTool(Name = "get_snapshot")]
    [Description("Return all windows recorded in a single snapshot.")]
    public object GetSnapshot(
        [Description("Snapshot id from list_snapshots.")] long snapshot_id)
    {
        var windows = db.GetWindowsForSnapshot(snapshot_id);
        return new { snapshot_id, count = windows.Count, windows };
    }

    [McpServerTool(Name = "list_apps")]
    [Description("List the distinct app_name values that have ever been captured. Useful as filter values for search_recall.")]
    public object ListApps() => new { apps = db.GetDistinctAppNames() };

    [McpServerTool(Name = "stats")]
    [Description("Database-wide stats: snapshot count, window-row count, total image bytes, first/last timestamps.")]
    public object Stats()
    {
        var s = db.GetStats();
        return new
        {
            snapshots = s.Snapshots,
            windows = s.Windows,
            image_bytes = s.ImageBytes,
            image_size_human = HumanBytes(s.ImageBytes),
            first_timestamp = s.FirstTimestamp,
            last_timestamp = s.LastTimestamp,
            db_path = db.DatabasePath,
        };
    }

    private static DateTimeOffset? ParseTs(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;
        if (DateTimeOffset.TryParse(s, out var dto)) return dto;
        return null;
    }

    private static string HumanBytes(long bytes)
    {
        string[] u = { "B", "KB", "MB", "GB", "TB" };
        double v = bytes; int i = 0;
        while (v >= 1024 && i < u.Length - 1) { v /= 1024; i++; }
        return $"{v:0.##} {u[i]}";
    }
}
