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

public partial class CaptureBar : UserControl
{
    private AppSettings? settings;

    public event EventHandler? StartClicked;
    public event EventHandler? StopClicked;

    public CaptureBar()
    {
        InitializeComponent();
        startBtn.Click += (_, _) => StartClicked?.Invoke(this, EventArgs.Empty);
        stopBtn.Click  += (_, _) => StopClicked?.Invoke(this, EventArgs.Empty);
        SetRunningState(false);
        lastLbl.Text = "Last: —";
    }

    public void BindSettings(AppSettings s)
    {
        settings = s;
        RefreshFromSettings();
    }

    public void RefreshFromSettings()
    {
        if (settings == null) return;
        var mode = settings.CaptureForegroundOnly ? "foreground" : "all windows";
        var change = settings.EnableChangeDetection ? " · skip unchanged" : "";
        var store = settings.StoreScreenshots ? "stored" : "OCR only";
        statsLbl.Text =
            $"Every {settings.IntervalSeconds}s  ·  JPEG q{settings.JpegQuality} ({store})  ·  {mode}{change}  ·  enc={settings.EncryptionMode}";
    }

    public void SetRunningState(bool running)
    {
        dotLbl.ForeColor = running ? Theme.Ok : Color.FromArgb(170, 170, 178);
        statusLbl.Text = running ? "Recording" : "Idle";
        statusLbl.ForeColor = running ? Theme.Ok : Theme.FgMuted;
        startBtn.Enabled = !running;
        stopBtn.Enabled  = running;
    }

    public void SetLastSnapshot(string text)
    {
        lastLbl.Text = text;
    }
}
