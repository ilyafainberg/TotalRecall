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
using System.Runtime.InteropServices;
using System.Text;

namespace TotalRecall;

/// <summary>
/// P/Invoke surface used by the capture pipeline. Grouped here so that
/// every native call has a single, audited definition.
/// </summary>
/// <remarks>
/// Two APIs do the heavy lifting:
/// <list type="bullet">
///   <item><see cref="EnumWindows"/> + <see cref="GetWindowText"/> etc. — drives
///     <see cref="WindowEnumerator"/>; cheap, no per-window cost.</item>
///   <item><see cref="PrintWindow"/> with <see cref="PW_RENDERFULLCONTENT"/> — drives
///     <see cref="ScreenshotCapture"/>; works for WPF, WinUI, Chromium-based apps
///     and other DirectComposition surfaces that older WM_PRINT-style capture misses.</item>
/// </list>
/// DWM calls are used to skip cloaked windows (UWP suspended/background instances)
/// and to query the visible frame bounds — <c>GetWindowRect</c> returns the legacy
/// frame which doesn't match the rendered window on Windows 10/11.
/// </remarks>
internal static class NativeMethods
{
    /// <summary>PrintWindow flag that asks the source app to fully render itself, not just blit its frame buffer.</summary>
    public const int PW_RENDERFULLCONTENT = 0x00000002;

    /// <summary>DWM attribute id for the cloaked state (UWP background, app-switcher previews, etc.).</summary>
    public const int DWMWA_CLOAKED = 14;

    public const int GWL_EXSTYLE = -20;

    /// <summary>Tool windows are auxiliary frames (palettes, tooltips); excluded from capture.</summary>
    public const int WS_EX_TOOLWINDOW = 0x00000080;

    public const int WS_EX_NOREDIRECTIONBITMAP = 0x00200000;

    public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll")]
    public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool IsIconic(IntPtr hWnd);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern int GetWindowTextLength(IntPtr hWnd);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll")]
    public static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
    private static extern int GetWindowLong32(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
    private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

    /// <summary>
    /// Bitness-agnostic wrapper. <c>GetWindowLongPtr</c> exists only on 64-bit Windows;
    /// 32-bit builds fall through to <c>GetWindowLong</c>.
    /// </summary>
    public static IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex) =>
        IntPtr.Size == 8 ? GetWindowLongPtr64(hWnd, nIndex) : new IntPtr(GetWindowLong32(hWnd, nIndex));

    [DllImport("user32.dll")]
    public static extern bool PrintWindow(IntPtr hWnd, IntPtr hdcBlt, int nFlags);

    [DllImport("dwmapi.dll")]
    public static extern int DwmGetWindowAttribute(IntPtr hWnd, int dwAttribute, out int pvAttribute, int cbAttribute);

    [DllImport("dwmapi.dll")]
    public static extern int DwmGetWindowAttribute(IntPtr hWnd, int dwAttribute, out RECT pvAttribute, int cbAttribute);

    /// <summary>DWM attribute id for the visible frame bounds (Win10/11 actual rendered rectangle).</summary>
    public const int DWMWA_EXTENDED_FRAME_BOUNDS = 9;

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public int Width => Right - Left;
        public int Height => Bottom - Top;
    }
}
