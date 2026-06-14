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
/// WinForms application entry point. Parses CLI arguments via <see cref="LaunchOptions"/>,
/// applies high-DPI + visual styles, then shows <see cref="MainForm"/>.
/// </summary>
/// <remarks>
/// Supported CLI args (see <see cref="LaunchOptions"/>):
/// <list type="bullet">
///   <item><c>--capture-off</c> — launch with capture paused (used by smoke tests and
///     by the auto-start shortcut so the app doesn't immediately start recording).</item>
///   <item><c>--minimized</c> — launch directly into the system tray.</item>
///   <item><c>--help</c> — show a usage MessageBox and exit.</item>
/// </list>
/// </remarks>
internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        var opts = LaunchOptions.Parse(args);

        if (opts.ShowHelp || opts.ParseError != null)
        {
            var msg = opts.ParseError != null
                ? opts.ParseError + Environment.NewLine + Environment.NewLine + LaunchOptions.HelpText()
                : LaunchOptions.HelpText();
            MessageBox.Show(msg, "TotalRecall",
                MessageBoxButtons.OK,
                opts.ParseError != null ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
            if (opts.ParseError != null) Environment.ExitCode = 1;
            if (opts.ShowHelp || opts.ParseError != null) return;
        }

        ApplicationConfiguration.Initialize();
        Application.SetColorMode(SystemColorMode.Classic);

        Application.Run(new MainForm(opts));
    }
}
