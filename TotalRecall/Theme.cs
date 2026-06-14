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
using System.Drawing;

namespace TotalRecall;

/// <summary>
/// Centralised colour + font palette for the WinForms UI. All custom drawing should pull
/// from this class so a future "dark mode" or accent re-skin only needs to touch one file.
/// </summary>
/// <remarks>
/// Designed to mimic the Fluent 2 / Windows 11 light theme: white "cards" floating on a
/// soft gray page, with a single accent blue and Segoe UI everywhere. If you add a colour,
/// add it here — don't hard-code <c>Color.FromArgb(…)</c> in form code.
/// </remarks>
internal static class Theme
{
    // Modern light palette: white cards on a soft light-gray page, Fluent accent.
    public static readonly Color Bg          = Color.FromArgb(0xF5, 0xF5, 0xF7);
    public static readonly Color BgPanel     = Color.FromArgb(0xFF, 0xFF, 0xFF);
    public static readonly Color BgRaised    = Color.FromArgb(0xEB, 0xEB, 0xEE);
    public static readonly Color Border      = Color.FromArgb(0xD2, 0xD2, 0xD7);
    public static readonly Color Fg          = Color.FromArgb(0x1C, 0x1C, 0x1E);
    public static readonly Color FgMuted     = Color.FromArgb(0x66, 0x66, 0x6C);
    public static readonly Color Accent      = Color.FromArgb(0x00, 0x78, 0xD4);
    public static readonly Color AccentHover = Color.FromArgb(0x10, 0x6E, 0xBE);
    public static readonly Color Danger      = Color.FromArgb(0xC4, 0x2B, 0x1C);
    public static readonly Color Ok          = Color.FromArgb(0x10, 0x7C, 0x10);

    public static readonly Font UiFont       = new("Segoe UI", 9.5f);
    public static readonly Font UiFontBold   = new("Segoe UI Semibold", 10f);
    public static readonly Font MonoFont     = new("Cascadia Mono", 9.5f, FontStyle.Regular, GraphicsUnit.Point);
}
