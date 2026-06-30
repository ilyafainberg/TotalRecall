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
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace TotalRecall;

/// <summary>
/// The "About TotalRecall" dialog. Replaces the old read-only MessageBox so the project
/// URL is a real clickable link and the author is credited.
/// </summary>
public sealed class AboutForm : Form
{
    private const string ProjectUrl = "https://github.com/ilyafainberg/TotalRecall";

    public AboutForm()
    {
        var ver = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "?";

        Text = "About TotalRecall";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Theme.BgPanel;
        ForeColor = Theme.Fg;
        Font = Theme.UiFont;
        ClientSize = new Size(420, 250);
        try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }

        var titleLbl = new Label
        {
            Text = $"TotalRecall  v{ver}",
            Font = new Font("Segoe UI Semibold", 13f),
            ForeColor = Theme.Fg,
            AutoSize = true,
            Location = new Point(24, 24),
        };

        var subtitleLbl = new Label
        {
            Text = "Local screen-activity indexer.",
            Font = Theme.UiFont,
            ForeColor = Theme.FgMuted,
            AutoSize = true,
            Location = new Point(24, 58),
        };

        var authorLbl = new Label
        {
            Text = "Author: Ilya Fainberg",
            Font = Theme.UiFontBold,
            ForeColor = Theme.Fg,
            AutoSize = true,
            Location = new Point(24, 96),
        };

        var linkCaptionLbl = new Label
        {
            Text = "Project:",
            ForeColor = Theme.FgMuted,
            AutoSize = true,
            Location = new Point(24, 128),
        };

        var link = new LinkLabel
        {
            Text = ProjectUrl,
            AutoSize = true,
            Location = new Point(80, 128),
            LinkColor = Theme.Accent,
            ActiveLinkColor = Theme.AccentHover,
            VisitedLinkColor = Theme.Accent,
        };
        link.LinkClicked += (_, _) => OpenUrl(ProjectUrl);

        var licenseLbl = new Label
        {
            Text = "Licensed under GPL-3.0-or-later.",
            ForeColor = Theme.FgMuted,
            AutoSize = true,
            Location = new Point(24, 160),
        };

        var copyrightLbl = new Label
        {
            Text = "Copyright (C) 2026 Ilya Fainberg.",
            ForeColor = Theme.FgMuted,
            AutoSize = true,
            Location = new Point(24, 184),
        };

        var okBtn = new Button
        {
            Text = "OK",
            DialogResult = DialogResult.OK,
            FlatStyle = FlatStyle.Flat,
            BackColor = Theme.BgRaised,
            ForeColor = Theme.Fg,
            Cursor = Cursors.Hand,
            Size = new Size(96, 32),
            Location = new Point(ClientSize.Width - 120, ClientSize.Height - 48),
            Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
        };
        okBtn.FlatAppearance.BorderColor = Theme.Border;

        Controls.Add(titleLbl);
        Controls.Add(subtitleLbl);
        Controls.Add(authorLbl);
        Controls.Add(linkCaptionLbl);
        Controls.Add(link);
        Controls.Add(licenseLbl);
        Controls.Add(copyrightLbl);
        Controls.Add(okBtn);

        AcceptButton = okBtn;
        CancelButton = okBtn;
    }

    private static void OpenUrl(string url)
    {
        try { Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true }); }
        catch { /* no default browser / blocked — nothing actionable for the user */ }
    }
}
