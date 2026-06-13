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
using Microsoft.Win32;

namespace TotalRecall;

/// <summary>
/// Manages the per-user "run at login" registry entry under
/// HKCU\Software\Microsoft\Windows\CurrentVersion\Run.
/// No admin rights needed; affects only the current Windows user.
/// </summary>
internal static class AutoStart
{
    private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "TotalRecall";

    private static string CurrentExePath =>
        Environment.ProcessPath ?? System.Windows.Forms.Application.ExecutablePath;

    public static bool IsEnabled()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKey, writable: false);
            return key?.GetValue(ValueName) is string s && !string.IsNullOrWhiteSpace(s);
        }
        catch { return false; }
    }

    /// <summary>Returns the command line currently registered, or null.</summary>
    public static string? GetCurrentValue()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKey, writable: false);
            return key?.GetValue(ValueName) as string;
        }
        catch { return null; }
    }

    /// <summary>Idempotent. Writes (or rewrites) the Run value so it always points at the current exe.</summary>
    public static void Enable()
    {
        using var key = Registry.CurrentUser.CreateSubKey(RunKey, writable: true)
                        ?? throw new InvalidOperationException("Could not open HKCU Run key.");
        var value = $"\"{CurrentExePath}\" --minimized";
        key.SetValue(ValueName, value, RegistryValueKind.String);
    }

    public static void Disable()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKey, writable: true);
        if (key == null) return;
        try { key.DeleteValue(ValueName, throwOnMissingValue: false); }
        catch { /* swallow — best effort */ }
    }

    /// <summary>Sync the registry to the desired state.</summary>
    public static void Set(bool enabled)
    {
        if (enabled) Enable();
        else Disable();
    }
}
