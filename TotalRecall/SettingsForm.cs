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
using System.Windows.Forms;

namespace TotalRecall;

/// <summary>
/// Non-modal host window for the existing SettingsPanel UserControl. Lets the user keep
/// browsing while editing settings; forwards Save/Purge/Clear events back to the caller
/// via plain event handlers so MainForm's wiring barely changes.
/// </summary>
public sealed class SettingsForm : Form
{
    public SettingsPanel Panel { get; }

    public event EventHandler? SettingsSaved;
    public event EventHandler? PurgeRequested;
    public event EventHandler? ClearDatabaseRequested;

    public SettingsForm(AppSettings settings)
    {
        Text = "TotalRecall — Settings";
        BackColor = Theme.Bg;
        ForeColor = Theme.Fg;
        Font = Theme.UiFont;
        Width = 980;
        Height = 760;
        MinimumSize = new Size(640, 480);
        StartPosition = FormStartPosition.CenterParent;
        KeyPreview = true;
        try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }

        Panel = new SettingsPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Theme.Bg,
            AutoScroll = true,
            Padding = new Padding(20),
        };
        Panel.BindSettings(settings);
        // Forward Save and close the window. The actual close is deferred to
        // the next message loop tick (BeginInvoke) so the panel's button click
        // handler fully unwinds before the form is disposed — closing during
        // the click handler invalidates the button mid-handler and can take
        // down the UI thread when the host then opens a follow-up modal.
        Panel.SettingsSaved += (_, _) =>
        {
            BeginInvoke(new Action(() =>
            {
                Close();
                SettingsSaved?.Invoke(this, EventArgs.Empty);
            }));
        };
        Panel.PurgeRequested += (_, _) => PurgeRequested?.Invoke(this, EventArgs.Empty);
        Panel.ClearDatabaseRequested += (_, _) => ClearDatabaseRequested?.Invoke(this, EventArgs.Empty);
        Panel.Cancelled += (_, _) => Close();

        // ESC closes (treated as Cancel) — natural keyboard UX for a settings
        // dialog. We don't set AcceptButton because Enter inside a text field
        // could trigger an accidental Save.
        KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Escape)
            {
                e.Handled = true;
                Close();
            }
        };

        Controls.Add(Panel);
    }
}
