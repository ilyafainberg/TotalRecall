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
using System.Text;
using System.Threading;
using static TotalRecall.NativeMethods;

namespace TotalRecall;

/// <summary>
/// Snapshot of one top-level window observed by <see cref="WindowEnumerator"/>.
/// </summary>
internal sealed record EnumeratedWindow(
    IntPtr Handle,
    string Title,
    int ProcessId,
    string ProcessName,
    string ExecutablePath,
    string AppName,
    RECT Bounds,
    bool IsForeground);

/// <summary>
/// Walks the desktop's top-level windows once per capture tick and filters out anything
/// that isn't worth screenshotting.
/// </summary>
/// <remarks>
/// Filter rules (in order, cheapest first — bail-out optimised):
/// <list type="number">
///   <item>Skip invisible windows (<c>IsWindowVisible</c> == false).</item>
///   <item>Skip minimised windows (<c>IsIconic</c>) — <c>PrintWindow</c> returns a
///     blank/old frame for these and OCR-ing it wastes cycles.</item>
///   <item>Skip DWM-cloaked windows (suspended UWP apps, app-switcher previews,
///     virtual-desktop ghosts).</item>
///   <item>Skip tool windows (<c>WS_EX_TOOLWINDOW</c>) — floating palettes etc.</item>
///   <item>Skip empty titles and windows smaller than 32×32.</item>
/// </list>
/// Process metadata (name, exe path, FileDescription/ProductName for friendly app name)
/// is resolved via <see cref="Process.GetProcessById"/> and cached for 5 minutes — looking
/// it up for every window on every tick is expensive (~50× the cost of the rest combined).
/// </remarks>
internal static class WindowEnumerator
{
    private static readonly Lock cacheLock = new();
    private static readonly Dictionary<int, CachedProcessInfo> processCache = new();
    private static DateTime lastCacheCleanupUtc = DateTime.MinValue;

    public static List<EnumeratedWindow> EnumerateTopLevelWindows()
    {
        var foreground = GetForegroundWindow();
        var results = new List<EnumeratedWindow>();

        EnumWindows((hWnd, _) =>
        {
            try
            {
                if (!IsWindowVisible(hWnd)) return true;
                if (IsIconic(hWnd)) return true; // skip minimized

                if (DwmGetWindowAttribute(hWnd, DWMWA_CLOAKED, out int cloaked, sizeof(int)) == 0 && cloaked != 0)
                    return true;

                var exStyle = (int)(long)GetWindowLongPtr(hWnd, GWL_EXSTYLE);
                if ((exStyle & WS_EX_TOOLWINDOW) != 0) return true;

                int len = GetWindowTextLength(hWnd);
                if (len <= 0) return true;

                var sb = new StringBuilder(len + 1);
                GetWindowText(hWnd, sb, sb.Capacity);
                var title = sb.ToString();
                if (string.IsNullOrWhiteSpace(title)) return true;

                if (!GetWindowRect(hWnd, out RECT rect)) return true;
                if (rect.Width < 32 || rect.Height < 32) return true;

                GetWindowThreadProcessId(hWnd, out uint pid);
                var processInfo = GetProcessInfo((int)pid);

                results.Add(new EnumeratedWindow(
                    hWnd, title, (int)pid, processInfo.ProcessName, processInfo.ExecutablePath, processInfo.AppName, rect, hWnd == foreground));
            }
            catch { /* skip windows that throw during interrogation — usually dying/transient frames */ }

            return true;
        }, IntPtr.Zero);

        return results;
    }

    /// <summary>
    /// Returns cached process metadata if seen in the last 5 minutes; otherwise queries
    /// the process and caches the result. Also evicts entries that haven't been touched
    /// in 10 minutes so the cache doesn't grow without bound across long sessions.
    /// </summary>
    private static CachedProcessInfo GetProcessInfo(int pid)
    {
        var now = DateTime.UtcNow;
        lock (cacheLock)
        {
            if (processCache.TryGetValue(pid, out var cached) && now - cached.LastSeenUtc < TimeSpan.FromMinutes(5))
            {
                var refreshed = cached with { LastSeenUtc = now };
                processCache[pid] = refreshed;
                return refreshed;
            }
        }

        var info = ResolveProcessInfo(pid, now);
        lock (cacheLock)
        {
            processCache[pid] = info;
            if (now - lastCacheCleanupUtc > TimeSpan.FromMinutes(5))
            {
                lastCacheCleanupUtc = now;
                foreach (var key in new List<int>(processCache.Keys))
                    if (now - processCache[key].LastSeenUtc > TimeSpan.FromMinutes(10))
                        processCache.Remove(key);
            }
        }
        return info;
    }

    /// <summary>
    /// Picks the most user-friendly name available, in this preference order:
    /// FileDescription → ProductName → ProcessName. Defensive against missing
    /// MainModule access (some protected processes throw on .NET).
    /// </summary>
    private static CachedProcessInfo ResolveProcessInfo(int pid, DateTime now)
    {
        string procName = "";
        string exePath = "";
        string appName = "";
        try
        {
            using var proc = Process.GetProcessById(pid);
            procName = proc.ProcessName;
            try { exePath = proc.MainModule?.FileName ?? ""; } catch { }
            if (!string.IsNullOrEmpty(exePath))
            {
                var fvi = FileVersionInfo.GetVersionInfo(exePath);
                appName = !string.IsNullOrWhiteSpace(fvi.FileDescription)
                    ? fvi.FileDescription!
                    : (!string.IsNullOrWhiteSpace(fvi.ProductName) ? fvi.ProductName! : procName);
            }
            else
            {
                appName = procName;
            }
        }
        catch { appName = procName; }
        return new CachedProcessInfo(procName, exePath, appName, now);
    }

    private sealed record CachedProcessInfo(string ProcessName, string ExecutablePath, string AppName, DateTime LastSeenUtc);
}
