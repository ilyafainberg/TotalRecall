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
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using static TotalRecall.NativeMethods;

namespace TotalRecall;

/// <summary>
/// Captures a single window's pixels via <c>PrintWindow(PW_RENDERFULLCONTENT)</c>.
/// </summary>
/// <remarks>
/// <para>Why <c>PrintWindow</c> + <c>PW_RENDERFULLCONTENT</c> instead of a desktop blit?</para>
/// <list type="bullet">
///   <item>Captures occluded / off-screen / partially covered windows.</item>
///   <item>No "minimised app shows the last frame" stale-frame artefact.</item>
///   <item>Works for DirectComposition surfaces (WPF, WinUI, Edge, modern UWP) where
///     plain <c>BitBlt</c> would return black.</item>
/// </list>
/// <para>Known limitations:</para>
/// <list type="bullet">
///   <item>Hardware-accelerated video / DRM-protected surfaces (Netflix, HDCP-protected
///     content) may render as black; not much we can do without admin/desktop-duplication.</item>
///   <item>Some legacy apps don't implement <c>WM_PRINT</c> cleanly and return a partial
///     frame; we fall back to <c>PrintWindow(0)</c> in that case before giving up.</item>
/// </list>
/// </remarks>
public static class ScreenshotCapture
{
    /// <summary>
    /// Captures the window referenced by <paramref name="hWnd"/>. Returns null when the
    /// window has no valid bounds or the capture call fails entirely (caller logs and continues).
    /// </summary>
    public static Bitmap? CaptureWindow(IntPtr hWnd)
    {
        // Prefer DWM extended frame bounds (gives the visual rectangle, not the legacy frame).
        // On Win10/11, GetWindowRect returns the larger "include drop-shadow / invisible
        // resize border" frame; DWMWA_EXTENDED_FRAME_BOUNDS is the visually-tight rectangle.
        RECT rect;
        if (DwmGetWindowAttribute(hWnd, DWMWA_EXTENDED_FRAME_BOUNDS, out rect, Marshal.SizeOf<RECT>()) != 0)
        {
            if (!GetWindowRect(hWnd, out rect)) return null;
        }

        int width = rect.Width;
        int height = rect.Height;
        if (width <= 0 || height <= 0) return null;

        var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        try
        {
            using var g = Graphics.FromImage(bmp);
            IntPtr hdc = g.GetHdc();
            try
            {
                bool ok = PrintWindow(hWnd, hdc, PW_RENDERFULLCONTENT);
                if (!ok)
                {
                    // Fallback: some legacy WM_PRINT implementations reject the flag.
                    if (!PrintWindow(hWnd, hdc, 0))
                    {
                        return null;
                    }
                }
            }
            finally
            {
                g.ReleaseHdc(hdc);
            }
            return bmp;
        }
        catch
        {
            bmp.Dispose();
            return null;
        }
    }
}
