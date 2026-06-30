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
using System.Windows.Forms;

namespace TotalRecall;

/// <summary>
/// A <see cref="SplitContainer"/> with a tidier splitter: the stock control shows the
/// chunky Win32 "split" cursor (the dumbbell/barbell glyph) over the splitter. This subclass
/// swaps in the standard <see cref="Cursors.SizeWE"/> / <see cref="Cursors.SizeNS"/> resize
/// cursor — and only over the splitter strip, never the panel contents.
/// </summary>
/// <remarks>
/// Implemented as a real type in the app assembly so the Visual Studio Designer can host
/// it (the BrowsePanel designer references this type for both split containers).
/// </remarks>
public class CleanSplitContainer : SplitContainer
{
    public CleanSplitContainer()
    {
        // Child controls inherit their parent's Cursor when they don't set their own. If we
        // put the resize cursor on the SplitContainer itself it would leak onto everything
        // inside the panels (the results list, the preview, etc.). So we pin BOTH panels to
        // the default arrow — their children inherit that — and only the bare splitter strip
        // (which is the SplitContainer's own uncovered surface) gets the resize cursor below.
        Panel1.Cursor = Cursors.Default;
        Panel2.Cursor = Cursors.Default;
        ApplySplitterCursor();
    }

    /// <summary>
    /// Swaps the chunky Win32 "barbell" split cursor on the splitter strip for the standard
    /// horizontal/vertical resize arrows, matching the splitter's orientation.
    /// </summary>
    private void ApplySplitterCursor()
        => Cursor = Orientation == Orientation.Vertical ? Cursors.SizeWE : Cursors.SizeNS;

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        ApplySplitterCursor();
    }
}
