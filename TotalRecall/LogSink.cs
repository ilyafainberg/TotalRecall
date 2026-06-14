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

namespace TotalRecall;

/// <summary>
/// Process-wide ring buffer for the activity log. Lets the activity log window be
/// opened/closed at any time without losing history, and lets multiple subscribers
/// (window + future status bar tickers) all listen in.
/// </summary>
internal static class LogSink
{
    private const int Capacity = 4000;
    private static readonly object gate = new();
    private static readonly Queue<string> buffer = new(Capacity);

    public static event EventHandler<string>? LineAppended;

    public static void Log(string message)
    {
        var line = $"[{DateTime.Now:HH:mm:ss}] {message}";
        lock (gate)
        {
            buffer.Enqueue(line);
            while (buffer.Count > Capacity) buffer.Dequeue();
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
}
