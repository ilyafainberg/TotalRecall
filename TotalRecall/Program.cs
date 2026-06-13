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
