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
using System.Windows.Forms;

namespace TotalRecall;

/// <summary>
/// A <see cref="TrackBar"/> that ignores the mouse wheel. The native Win32 trackbar
/// scrolls on <c>WM_MOUSEWHEEL</c> all by itself, which makes it far too easy to nudge
/// the JPEG-quality slider by accident while scrolling the Settings page. Swallowing the
/// message keeps the slider value stable; users change it by click/drag or arrow keys.
/// </summary>
/// <remarks>
/// Lives in the app assembly so the Visual Studio Designer can instantiate it directly
/// (the Settings designer references this type for <c>qualityBar</c>).
/// </remarks>
public class NoScrollTrackBar : TrackBar
{
    private const int WM_MOUSEWHEEL = 0x020A;

    protected override void WndProc(ref Message m)
    {
        // Drop wheel messages before the native control sees them.
        if (m.Msg == WM_MOUSEWHEEL) return;
        base.WndProc(ref m);
    }
}
