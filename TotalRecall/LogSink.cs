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
using System.IO;

namespace TotalRecall;

/// <summary>
/// Process-wide ring buffer for the activity log. Lets the activity log window be
/// opened/closed at any time without losing history, and lets multiple subscribers
/// (window + future status bar tickers) all listen in. Also mirrors every line to
/// <c>%LOCALAPPDATA%\TotalRecall\app.log</c> (rolled at ~1 MB) so we can post-mortem
/// crashes even when the activity-log window was never opened during the session.
/// </summary>
internal static class LogSink
{
    private const int Capacity = 4000;
    private const long FileRollBytes = 1L * 1024 * 1024;
    private static readonly object gate = new();
    private static readonly Queue<string> buffer = new(Capacity);
    private static readonly string filePath = InitFilePath();

    public static event EventHandler<string>? LineAppended;

    private static string InitFilePath()
    {
        try
        {
            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TotalRecall");
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, "app.log");
        }
        catch { return ""; }
    }

    public static void Log(string message)
    {
        var line = $"[{DateTime.Now:HH:mm:ss}] {message}";
        lock (gate)
        {
            buffer.Enqueue(line);
            while (buffer.Count > Capacity) buffer.Dequeue();
            WriteToFile(line);
        }
        try { LineAppended?.Invoke(null, line); } catch { /* sinks must not break callers */ }
    }

    public static IReadOnlyList<string> Snapshot()
    {
        lock (gate) return buffer.ToArray();
    }

    public static void Clear()
    {
        lock (gate) buffer.Clear();
    }

    /// <summary>
    /// Best-effort append to the persistent log file. Silently swallows IO errors —
    /// the in-memory ring is the primary store and logging must never break callers.
    /// Rolls the file in half when it crosses <see cref="FileRollBytes"/> so the
    /// log can't grow unbounded across long-running sessions.
    /// </summary>
    private static void WriteToFile(string line)
    {
        if (string.IsNullOrEmpty(filePath)) return;
        try
        {
            File.AppendAllText(filePath, line + Environment.NewLine);
            var info = new FileInfo(filePath);
            if (info.Exists && info.Length > FileRollBytes)
            {
                // Cheap rolling: keep the second half of the file.
                var bytes = File.ReadAllBytes(filePath);
                var keep = bytes.Length / 2;
                File.WriteAllBytes(filePath, bytes[^keep..]);
            }
        }
        catch { /* disk full, OneDrive lock, etc. — drop silently */ }
    }
}
