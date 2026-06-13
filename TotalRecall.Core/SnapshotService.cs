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
using System.Drawing.Imaging;
using System.Threading;
using System.Threading.Tasks;

namespace TotalRecall;

public sealed class SnapshotService
{
    private readonly OcrService ocr;
    private readonly Database db;
    private readonly Func<AppSettings> settingsProvider;
    private readonly Dictionary<string, ulong> lastWindowHashes = new(StringComparer.Ordinal);

    public SnapshotService(OcrService ocr, Database db, Func<AppSettings> settingsProvider)
    {
        this.ocr = ocr;
        this.db = db;
        this.settingsProvider = settingsProvider;
    }

    public sealed record SnapshotResult(long SnapshotId, int WindowCount, int StoredWindowCount, int SkippedUnchanged, long ElapsedMs, long ImageBytes);

    public async Task<SnapshotResult> CaptureOnceAsync(CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        var timestamp = DateTimeOffset.Now;
        var user = Environment.UserDomainName + "\\" + Environment.UserName;
        var machine = Environment.MachineName;
        var settings = settingsProvider();
        var jpegQuality = settings.JpegQuality;
        var storeImages = settings.StoreScreenshots;
        var enableChangeDetection = settings.EnableChangeDetection;
        var excluded = BuildExclusionTokens(settings.ExcludedApps);

        var windows = WindowEnumerator.EnumerateTopLevelWindows();
        if (settings.CaptureForegroundOnly)
            windows = windows.FindAll(w => w.IsForeground);
        if (excluded.Count > 0)
            windows = windows.FindAll(w => !IsExcluded(w, excluded));

        var captured = new List<CapturedWindow>(windows.Count);
        long totalImageBytes = 0;
        long snapshotId = 0;
        int skippedUnchanged = 0;

        await Task.Run(() =>
        {
            ct.ThrowIfCancellationRequested();

            foreach (var w in windows)
            {
                ct.ThrowIfCancellationRequested();
                var record = new WindowRecord
                {
                    Title = w.Title,
                    AppName = w.AppName,
                    ProcessName = w.ProcessName ?? "",
                    ProcessId = w.ProcessId,
                    ExecutablePath = w.ExecutablePath,
                    IsForeground = w.IsForeground,
                    Bounds = new WindowBounds
                    {
                        X = w.Bounds.Left, Y = w.Bounds.Top,
                        Width = w.Bounds.Width, Height = w.Bounds.Height,
                    },
                };

                using var bmp = ScreenshotCapture.CaptureWindow(w.Handle);
                byte[]? jpeg = null;

                if (bmp == null)
                {
                    record.OcrError = "Screenshot failed";
                }
                else
                {
                    if (enableChangeDetection)
                    {
                        var hash = ComputeVisualHash(bmp);
                        var key = BuildWindowKey(w);
                        if (lastWindowHashes.TryGetValue(key, out var previousHash) && previousHash == hash)
                        {
                            skippedUnchanged++;
                            continue;
                        }
                        lastWindowHashes[key] = hash;
                    }

                    // OCR on the original (lossless) bitmap for best text recovery.
                    var ocrSw = Stopwatch.StartNew();
                    try { record.Text = ocr.Recognize(bmp); }
                    catch (Exception ex) { record.OcrError = ex.Message; }
                    ocrSw.Stop();
                    record.OcrDurationMs = (int)ocrSw.ElapsedMilliseconds;

                    if (storeImages)
                    {
                        try
                        {
                            jpeg = JpegEncoder.Encode(bmp, jpegQuality);
                            totalImageBytes += jpeg.Length;
                        }
                        catch (Exception ex)
                        {
                            record.OcrError = (record.OcrError ?? "") + " | JPEG encode failed: " + ex.Message;
                        }
                    }
                }

                captured.Add(new CapturedWindow(record, jpeg));
            }

            if (captured.Count > 0)
                snapshotId = db.InsertSnapshotWithWindows(timestamp, user, machine, windows.Count, sw.ElapsedMilliseconds, captured);
        }, ct);

        sw.Stop();
        return new SnapshotResult(snapshotId, windows.Count, captured.Count, skippedUnchanged, sw.ElapsedMilliseconds, totalImageBytes);
    }

    private static List<string> BuildExclusionTokens(string raw)
    {
        var tokens = new List<string>();
        foreach (var token in raw.Split(new[] { '\r', '\n', ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            if (token.Length > 0) tokens.Add(token);
        return tokens;
    }

    private static bool IsExcluded(EnumeratedWindow window, List<string> tokens)
    {
        foreach (var token in tokens)
        {
            if (Contains(window.AppName, token) ||
                Contains(window.ProcessName, token) ||
                Contains(window.Title, token) ||
                Contains(window.ExecutablePath, token))
                return true;
        }
        return false;
    }

    private static bool Contains(string value, string token) =>
        value.Contains(token, StringComparison.OrdinalIgnoreCase);

    private static string BuildWindowKey(EnumeratedWindow w) =>
        $"{w.ProcessId}|{w.Title}|{w.Bounds.Left},{w.Bounds.Top},{w.Bounds.Width},{w.Bounds.Height}";

    private static unsafe ulong ComputeVisualHash(Bitmap bmp)
    {
        const int sampleColumns = 32;
        const int sampleRows = 18;
        const ulong offset = 14695981039346656037UL;
        const ulong prime = 1099511628211UL;

        int width = bmp.Width;
        int height = bmp.Height;

        // Lock the pixels once and read the sample grid directly from the bitmap
        // buffer. Format32bppArgb stores BGRA little-endian, so a 32-bit read at a
        // pixel yields the same 0xAARRGGBB value Color.ToArgb() returned before.
        var data = bmp.LockBits(new Rectangle(0, 0, width, height),
            ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        try
        {
            byte* scan0 = (byte*)data.Scan0;
            int stride = data.Stride;

            unchecked
            {
                ulong hash = offset;
                hash = (hash ^ (uint)width) * prime;
                hash = (hash ^ (uint)height) * prime;
                for (int y = 0; y < sampleRows; y++)
                {
                    var py = Math.Min(height - 1, y * height / sampleRows);
                    byte* row = scan0 + (long)py * stride;
                    for (int x = 0; x < sampleColumns; x++)
                    {
                        var px = Math.Min(width - 1, x * width / sampleColumns);
                        uint argb = *(uint*)(row + (px << 2));
                        hash = (hash ^ argb) * prime;
                    }
                }
                return hash;
            }
        }
        finally
        {
            bmp.UnlockBits(data);
        }
    }
}
