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
using System.Text;
using System.Windows.Forms;

namespace TotalRecall;

/// <summary>
/// Non-modal "Activity log" window opened from the hamburger menu. Subscribes to
/// <see cref="LogSink.LineAppended"/> and renders new lines into a read-only multiline
/// TextBox. Closing the window just hides it — <see cref="MainForm"/> reuses the same
/// instance so the scrollback survives reopens.
/// </summary>
public sealed class ActivityLogForm : Form
{
    // Keep the on-screen scrollback in lock-step with LogSink's ring buffer so the
    // window never accumulates more than the documented number of lines.
    private const int Capacity = 4000;

    private readonly TextBox logTxt;
    private readonly Label countLbl;

    public ActivityLogForm()
    {
        Text = "TotalRecall — Activity log";
        BackColor = Theme.Bg;
        ForeColor = Theme.Fg;
        Font = Theme.UiFont;
        Width = 900;
        Height = 560;
        MinimumSize = new Size(540, 320);
        StartPosition = FormStartPosition.CenterParent;
        try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }

        // Construct the fields the lambdas below close over before wiring those lambdas
        // up so the nullable flow analysis sees them as definitely assigned.
        logTxt = new TextBox
        {
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Both,
            WordWrap = false,
            Dock = DockStyle.Fill,
            BackColor = Theme.BgPanel,
            ForeColor = Theme.Fg,
            BorderStyle = BorderStyle.None,
            Font = Theme.MonoFont,
        };
        countLbl = new Label
        {
            Dock = DockStyle.Fill,
            ForeColor = Theme.FgMuted,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(4, 0, 4, 0),
        };

        var footer = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 44,
            BackColor = Theme.BgRaised,
            Padding = new Padding(12, 8, 12, 8),
        };

        var clearBtn = new Button
        {
            Text = "Clear",
            FlatStyle = FlatStyle.Flat,
            BackColor = Theme.BgPanel,
            ForeColor = Theme.Fg,
            Cursor = Cursors.Hand,
            Width = 90,
            Height = 28,
            Dock = DockStyle.Right,
            Font = Theme.UiFontBold,
        };
        clearBtn.FlatAppearance.BorderColor = Theme.Border;
        clearBtn.Click += (_, _) =>
        {
            LogSink.Clear();
            logTxt.Clear();
            UpdateCount();
        };

        var copyBtn = new Button
        {
            Text = "Copy all",
            FlatStyle = FlatStyle.Flat,
            BackColor = Theme.BgPanel,
            ForeColor = Theme.Fg,
            Cursor = Cursors.Hand,
            Width = 90,
            Height = 28,
            Dock = DockStyle.Right,
            Font = Theme.UiFontBold,
        };
        copyBtn.FlatAppearance.BorderColor = Theme.Border;
        copyBtn.Click += (_, _) =>
        {
            try { if (logTxt.TextLength > 0) Clipboard.SetText(logTxt.Text); } catch { }
        };

        countLbl = new Label
        {
            Dock = DockStyle.Fill,
            ForeColor = Theme.FgMuted,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(4, 0, 4, 0),
        };

        footer.Controls.Add(countLbl);
        footer.Controls.Add(copyBtn);
        footer.Controls.Add(clearBtn);

        var body = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0), BackColor = Theme.BgPanel };
        body.Controls.Add(logTxt);

        Controls.Add(body);
        Controls.Add(footer);

        LoadInitial();
        LogSink.LineAppended += OnLineAppended;
        FormClosed += (_, _) => LogSink.LineAppended -= OnLineAppended;
    }

    private void LoadInitial()
    {
        var snap = LogSink.Snapshot();
        if (snap.Count == 0) { UpdateCount(); return; }
        var sb = new StringBuilder();
        foreach (var line in snap) sb.AppendLine(line);
        logTxt.Text = sb.ToString();
        logTxt.SelectionStart = logTxt.TextLength;
        logTxt.ScrollToCaret();
        UpdateCount();
    }

    private void OnLineAppended(object? sender, string line)
    {
        if (IsDisposed) return;
        if (InvokeRequired) { BeginInvoke(new Action<object?, string>(OnLineAppended), sender, line); return; }
        var atBottom = logTxt.SelectionStart >= logTxt.TextLength - 1;
        logTxt.AppendText(line + Environment.NewLine);
        TrimToCapacity();
        if (atBottom) { logTxt.SelectionStart = logTxt.TextLength; logTxt.ScrollToCaret(); }
        UpdateCount();
    }

    /// <summary>
    /// Drops the oldest lines so the TextBox holds at most <see cref="Capacity"/> entries.
    /// Without this the box grows without bound even though <see cref="LogSink"/> caps its
    /// own ring buffer — the window would drift to 4,001+ lines over a long session.
    /// </summary>
    private void TrimToCapacity()
    {
        // Lines includes a trailing empty element because the text ends in a newline.
        int contentLines = ContentLineCount();
        if (contentLines <= Capacity) return;

        int remove = contentLines - Capacity;
        var text = logTxt.Text;
        int idx = 0;
        for (int i = 0; i < remove; i++)
        {
            int nl = text.IndexOf('\n', idx);
            if (nl < 0) { idx = text.Length; break; }
            idx = nl + 1;
        }
        logTxt.Text = text[idx..];
        logTxt.SelectionStart = logTxt.TextLength;
    }

    private int ContentLineCount()
    {
        if (logTxt.TextLength == 0) return 0;
        int n = logTxt.Lines.Length;
        // A trailing newline produces a final empty Lines element — don't count it.
        if (logTxt.Text.EndsWith('\n')) n--;
        return n;
    }

    private void UpdateCount()
    {
        countLbl.Text = $"{ContentLineCount():N0} lines · ring buffer keeps the last {Capacity:N0}";
    }
}
