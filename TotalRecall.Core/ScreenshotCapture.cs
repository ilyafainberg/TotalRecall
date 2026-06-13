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

public static class ScreenshotCapture
{
    /// <summary>
    /// Captures a window using PrintWindow with PW_RENDERFULLCONTENT (works for most modern apps,
    /// including those using DirectComposition). Returns null on failure.
    /// </summary>
    public static Bitmap? CaptureWindow(IntPtr hWnd)
    {
        // Prefer DWM extended frame bounds (gives the visual rectangle, not the legacy frame).
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
                    // Fallback: try without flag
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
