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
using System.Text;

namespace TotalRecall;

/// <summary>
/// Parsed command-line launch options.
/// </summary>
public sealed class LaunchOptions
{
    /// <summary>Minimize the main window on launch (`--minimized`, `/minimized`, `-m`).</summary>
    public bool StartMinimized { get; set; }

    /// <summary>Hide the main window into the system tray on launch (`--tray`, `/tray`). Overrides MinimizeToTray setting.</summary>
    public bool StartInTray { get; set; }

    /// <summary>
    /// Forces the initial capture state. `true` = start capture regardless of remembered state,
    /// `false` = do NOT start capture regardless of remembered state, `null` = use remembered
    /// <see cref="AppSettings.WasCapturing"/>.
    /// </summary>
    public bool? CaptureOverride { get; set; }

    /// <summary>True if the user asked for `--help` / `-h` / `-?` / `/?`.</summary>
    public bool ShowHelp { get; set; }

    /// <summary>Unknown / malformed arguments collected during parsing.</summary>
    public string? ParseError { get; set; }

    public static LaunchOptions Parse(string[] args)
    {
        var opts = new LaunchOptions();
        foreach (var raw in args)
        {
            var a = raw?.Trim() ?? "";
            if (a.Length == 0) continue;

            if (Eq(a, "--minimized", "/minimized", "-m"))
            {
                opts.StartMinimized = true;
            }
            else if (Eq(a, "--tray", "/tray"))
            {
                opts.StartInTray = true;
            }
            else if (Eq(a, "--capture-on", "/capture-on", "--capture=on", "/capture=on"))
            {
                if (opts.CaptureOverride == false)
                {
                    opts.ParseError = "Cannot specify both --capture-on and --capture-off.";
                    return opts;
                }
                opts.CaptureOverride = true;
            }
            else if (Eq(a, "--capture-off", "/capture-off", "--capture=off", "/capture=off"))
            {
                if (opts.CaptureOverride == true)
                {
                    opts.ParseError = "Cannot specify both --capture-on and --capture-off.";
                    return opts;
                }
                opts.CaptureOverride = false;
            }
            else if (Eq(a, "--help", "-h", "-?", "/?", "/help"))
            {
                opts.ShowHelp = true;
            }
            else
            {
                opts.ParseError = $"Unknown argument: {a}";
                return opts;
            }
        }
        return opts;
    }

    public static string HelpText()
    {
        var sb = new StringBuilder();
        sb.AppendLine("TotalRecall — local screen-activity indexer.");
        sb.AppendLine("Usage:  TotalRecall.exe [options]");
        sb.AppendLine();
        sb.AppendLine("Window:");
        sb.AppendLine("  --minimized, -m         Launch with the window minimized.");
        sb.AppendLine("  --tray                  Launch hidden in the system tray (overrides the");
        sb.AppendLine("                          'Minimize to tray' setting). Right-click the tray");
        sb.AppendLine("                          icon or double-click it to restore the window.");
        sb.AppendLine();
        sb.AppendLine("Capture (overrides the remembered on/off state):");
        sb.AppendLine("  --capture-on            Start capture immediately, regardless of last state.");
        sb.AppendLine("  --capture-off           Do not start capture, regardless of last state.");
        sb.AppendLine();
        sb.AppendLine("Other:");
        sb.AppendLine("  --help, -h, /?          Show this message and exit.");
        sb.AppendLine();
        sb.AppendLine("Examples:");
        sb.AppendLine("  TotalRecall.exe --tray --capture-on        Silent background capture.");
        sb.AppendLine("  TotalRecall.exe --minimized --capture-off  Open minimized, idle.");
        return sb.ToString();
    }

    private static bool Eq(string a, params string[] options)
    {
        foreach (var o in options)
            if (string.Equals(a, o, StringComparison.OrdinalIgnoreCase)) return true;
        return false;
    }
}
