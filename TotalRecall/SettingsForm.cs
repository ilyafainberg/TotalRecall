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
        try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }

        Panel = new SettingsPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Theme.Bg,
            AutoScroll = true,
            Padding = new Padding(20),
        };
        Panel.BindSettings(settings);
        Panel.SettingsSaved += (_, _) => SettingsSaved?.Invoke(this, EventArgs.Empty);
        Panel.PurgeRequested += (_, _) => PurgeRequested?.Invoke(this, EventArgs.Empty);
        Panel.ClearDatabaseRequested += (_, _) => ClearDatabaseRequested?.Invoke(this, EventArgs.Empty);

        Controls.Add(Panel);
    }
}
