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

// ============================================================================
//  Public DTO / record types shared by all three projects:
//    - TotalRecall.Core   : produces them (capture + persistence)
//    - TotalRecall        : consumes them in the WinForms UI
//    - TotalRecall.Mcp    : returns them as JSON over the MCP transport
//
//  Keep these intentionally thin (no behaviour). Adding a heavy method here
//  forces every consumer to rebuild — and the MCP project serializes these
//  to JSON, so any property added becomes part of the public tool schema.
// ============================================================================

/// <summary>
/// A single "tick" of the capture loop. Captures a timestamp, the user/machine
/// that ran the capture, and the per-window state observed during the tick.
/// </summary>
public sealed class Snapshot
{
    public DateTimeOffset Timestamp { get; set; }
    public string User { get; set; } = "";
    public string Machine { get; set; } = "";
    public List<WindowRecord> Windows { get; set; } = new();
}

/// <summary>
/// One captured top-level window: identity (title + app + pid), placement
/// (<see cref="Bounds"/>), captured pixels (<see cref="ScreenshotPath"/> at
/// the in-memory level), and the OCR result (<see cref="Text"/>, with any
/// <see cref="OcrError"/> + duration metric).
/// </summary>
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
    /// <summary>Reserved for callers that persist screenshots to disk instead of the DB blob.</summary>
    public string? ScreenshotPath { get; set; }
    public int OcrDurationMs { get; set; }
}

/// <summary>Screen-space rectangle for a captured window (DWM extended frame).</summary>
public sealed class WindowBounds
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
}

/// <summary>Outcome of one retention pass: how many JPEG blobs were stripped and how many rows were deleted.</summary>
public readonly record struct RetentionResult(int ImagesPurged, int RowsDeleted)
{
    /// <summary>True when retention had any effect — used to decide whether to follow up with a VACUUM.</summary>
    public bool Changed => ImagesPurged > 0 || RowsDeleted > 0;
}

/// <summary>Outcome of a database compaction (VACUUM). <c>Ran=false</c> means the call was a no-op.</summary>
public readonly record struct CompactionResult(bool Ran, long BeforeBytes, long AfterBytes)
{
    public long ReclaimedBytes => Math.Max(0, BeforeBytes - AfterBytes);
}
