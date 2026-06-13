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

public sealed class Snapshot
{
    public DateTimeOffset Timestamp { get; set; }
    public string User { get; set; } = "";
    public string Machine { get; set; } = "";
    public List<WindowRecord> Windows { get; set; } = new();
}

public sealed class WindowRecord
{
    public string Title { get; set; } = "";
    public string AppName { get; set; } = "";
    public string ProcessName { get; set; } = "";
    public int ProcessId { get; set; }
    public string ExecutablePath { get; set; } = "";
    public WindowBounds? Bounds { get; set; }
    public bool IsForeground { get; set; }
    public string Text { get; set; } = "";
    public string? OcrError { get; set; }
    public string? ScreenshotPath { get; set; }
    public int OcrDurationMs { get; set; }
}

public sealed class WindowBounds
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
}

public readonly record struct RetentionResult(int ImagesPurged, int RowsDeleted)
{
    public bool Changed => ImagesPurged > 0 || RowsDeleted > 0;
}

public readonly record struct CompactionResult(bool Ran, long BeforeBytes, long AfterBytes)
{
    public long ReclaimedBytes => Math.Max(0, BeforeBytes - AfterBytes);
}
