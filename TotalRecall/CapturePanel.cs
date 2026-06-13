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

public partial class CapturePanel : UserControl
{
    private AppSettings? settings;

    public event EventHandler? StartClicked;
    public event EventHandler? StopClicked;
    public event EventHandler? CaptureNowClicked;
    public event EventHandler? OpenDbFolderClicked;

    public CapturePanel()
    {
        InitializeComponent();
        WireEvents();
    }

    public CapturePanel(AppSettings settings) : this()
    {
        BindSettings(settings);
    }

    private void WireEvents()
    {
        startBtn.Click += (_, _) => StartClicked?.Invoke(this, EventArgs.Empty);
        stopBtn.Click += (_, _) => StopClicked?.Invoke(this, EventArgs.Empty);
        onceBtn.Click += (_, _) => CaptureNowClicked?.Invoke(this, EventArgs.Empty);
        openDbBtn.Click += (_, _) => OpenDbFolderClicked?.Invoke(this, EventArgs.Empty);
    }

    public void BindSettings(AppSettings settings)
    {
        this.settings = settings;
        RefreshFromSettings();
    }

    public void RefreshFromSettings()
    {
        if (settings == null) return;
        intervalLbl.Text = $"{settings.IntervalSeconds} s";
        var mode = settings.CaptureForegroundOnly ? "foreground" : "all windows";
        var changeDetection = settings.EnableChangeDetection ? ", skip unchanged" : "";
        qualityLbl.Text  = $"{settings.JpegQuality} ({(settings.StoreScreenshots ? "stored" : "OCR only")}, {mode}{changeDetection})";
        encLbl.Text      = settings.EncryptionMode.ToString();
    }

    public void SetRunningState(bool running)
    {
        startBtn.Enabled = !running;
        onceBtn.Enabled = !running;
        stopBtn.Enabled = running;
    }

    public void Log(string msg)
    {
        if (InvokeRequired) { BeginInvoke(() => Log(msg)); return; }
        logTxt.AppendText(msg + Environment.NewLine);
        if (logTxt.Lines.Length > 5000) logTxt.Lines = logTxt.Lines[^2500..];
    }

    /// <summary>
    /// Utility used by other panels in the project (e.g. SettingsPanel, BrowsePanel)
    /// to build a button that matches the panel's flat-dark style.
    /// </summary>
    public static Button MakeButton(string text, Color bg, Color fg)
    {
        var b = new Button
        {
            Text = text, Height = 36,
            FlatStyle = FlatStyle.Flat,
            BackColor = bg, ForeColor = fg,
            Font = new Font("Segoe UI Semibold", 9.5f),
            UseVisualStyleBackColor = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Padding = new Padding(8, 0, 8, 0),
            Cursor = Cursors.Hand,
        };
        b.FlatAppearance.BorderColor = Theme.Border;
        b.FlatAppearance.BorderSize = 1;
        return b;
    }
}
