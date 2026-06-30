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
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace TotalRecall;

/// <summary>Metadata about an available newer release, resolved from the GitHub Releases API.</summary>
public sealed record UpdateInfo(
    Version Version,
    string TagName,
    string AssetName,
    string DownloadUrl,
    long SizeBytes,
    string ReleaseUrl);

/// <summary>
/// Checks the GitHub Releases API for a newer TotalRecall build and downloads the
/// portable asset with progress reporting. No auto-install: the caller decides what to do
/// with the downloaded archive (the app surfaces it to the user and offers to open it).
/// </summary>
public static class UpdateService
{
    public const string Owner = "ilyafainberg";
    public const string Repo = "TotalRecall";

    private static readonly Uri LatestReleaseApi =
        new($"https://api.github.com/repos/{Owner}/{Repo}/releases/latest");

    // GitHub requires a User-Agent on every API request.
    private static HttpClient CreateClient()
    {
        var http = new HttpClient { Timeout = TimeSpan.FromMinutes(10) };
        http.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("TotalRecall-Updater", "1.0"));
        http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        return http;
    }

    /// <summary>
    /// Queries the latest release. Returns an <see cref="UpdateInfo"/> when the published
    /// version is strictly newer than <paramref name="current"/>, otherwise <c>null</c>.
    /// </summary>
    /// <exception cref="Exception">Network / parse failures bubble up for the caller to surface.</exception>
    public static async Task<UpdateInfo?> CheckForUpdateAsync(Version current, CancellationToken ct = default)
    {
        using var http = CreateClient();
        using var resp = await http.GetAsync(LatestReleaseApi, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        var json = await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var tag = root.TryGetProperty("tag_name", out var t) ? t.GetString() ?? "" : "";
        var releaseUrl = root.TryGetProperty("html_url", out var h) ? h.GetString() ?? "" : "";
        var latest = ParseVersion(tag);
        if (latest == null || latest <= current) return null;

        // Prefer the portable win-x64 zip; fall back to the first .zip asset.
        string assetName = "", downloadUrl = "";
        long size = 0;
        if (root.TryGetProperty("assets", out var assets) && assets.ValueKind == JsonValueKind.Array)
        {
            var list = assets.EnumerateArray().ToList();
            var chosen = list.FirstOrDefault(a => Name(a).Contains("Portable", StringComparison.OrdinalIgnoreCase)
                                                  && Name(a).EndsWith(".zip", StringComparison.OrdinalIgnoreCase));
            if (chosen.ValueKind != JsonValueKind.Object)
                chosen = list.FirstOrDefault(a => Name(a).EndsWith(".zip", StringComparison.OrdinalIgnoreCase));

            if (chosen.ValueKind == JsonValueKind.Object)
            {
                assetName = Name(chosen);
                downloadUrl = chosen.TryGetProperty("browser_download_url", out var u) ? u.GetString() ?? "" : "";
                size = chosen.TryGetProperty("size", out var s) ? s.GetInt64() : 0;
            }
        }

        if (string.IsNullOrEmpty(downloadUrl)) return null;
        return new UpdateInfo(latest, tag, assetName, downloadUrl, size, releaseUrl);

        static string Name(JsonElement a) => a.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "";
    }

    /// <summary>
    /// Downloads <paramref name="info"/> to a temp file and returns the path. Reports
    /// fractional progress (0..1) when the server sends a content length, otherwise reports
    /// -1 (indeterminate).
    /// </summary>
    public static async Task<string> DownloadAsync(UpdateInfo info, IProgress<double>? progress, CancellationToken ct = default)
    {
        using var http = CreateClient();
        using var resp = await http.GetAsync(info.DownloadUrl, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();

        var total = resp.Content.Headers.ContentLength ?? info.SizeBytes;
        var dir = Path.Combine(Path.GetTempPath(), "TotalRecall-Update");
        Directory.CreateDirectory(dir);
        var dest = Path.Combine(dir, string.IsNullOrEmpty(info.AssetName) ? $"TotalRecall-{info.TagName}.zip" : info.AssetName);

        await using (var src = await resp.Content.ReadAsStreamAsync(ct).ConfigureAwait(false))
        await using (var dst = new FileStream(dest, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            var buffer = new byte[81920];
            long read = 0;
            int n;
            while ((n = await src.ReadAsync(buffer, ct).ConfigureAwait(false)) > 0)
            {
                await dst.WriteAsync(buffer.AsMemory(0, n), ct).ConfigureAwait(false);
                read += n;
                if (total > 0) progress?.Report((double)read / total);
                else progress?.Report(-1);
            }
        }

        return dest;
    }

    /// <summary>Parses a GitHub tag like <c>v1.2.3</c> or <c>1.2.3</c> into a <see cref="Version"/>.</summary>
    private static Version? ParseVersion(string? tag)
    {
        if (string.IsNullOrWhiteSpace(tag)) return null;
        var s = tag.Trim();
        if (s.StartsWith('v') || s.StartsWith('V')) s = s[1..];
        // Drop any pre-release suffix (e.g. 1.2.3-beta).
        var dash = s.IndexOf('-');
        if (dash >= 0) s = s[..dash];
        return Version.TryParse(s, out var v) ? v : null;
    }
}
