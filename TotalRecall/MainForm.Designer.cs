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
namespace TotalRecall;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;

    private void InitializeComponent()
    {
        headerPanel = new Panel();
        quitBtn = new Button();
        titleLbl = new Label();
        tabs = new TabControl();
        tabCapture = new TabPage();
        capturePanel = new CapturePanel();
        tabBrowse = new TabPage();
        browsePanel = new BrowsePanel();
        tabSettings = new TabPage();
        settingsPanel = new SettingsPanel();
        statusPanel = new Panel();
        dbLbl = new Label();
        statusLbl = new Label();
        headerPanel.SuspendLayout();
        tabs.SuspendLayout();
        tabCapture.SuspendLayout();
        tabBrowse.SuspendLayout();
        tabSettings.SuspendLayout();
        statusPanel.SuspendLayout();
        SuspendLayout();
        // 
        // headerPanel
        // 
        headerPanel.BackColor = Color.FromArgb(235, 235, 238);
        headerPanel.Controls.Add(quitBtn);
        headerPanel.Controls.Add(titleLbl);
        headerPanel.Dock = DockStyle.Top;
        headerPanel.Location = new Point(0, 0);
        headerPanel.Name = "headerPanel";
        headerPanel.Padding = new Padding(16, 10, 16, 10);
        headerPanel.Size = new Size(1484, 56);
        headerPanel.TabIndex = 2;
        // 
        // quitBtn
        // 
        quitBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        quitBtn.BackColor = Color.FromArgb(245, 245, 247);
        quitBtn.Cursor = Cursors.Hand;
        quitBtn.FlatAppearance.BorderColor = Color.FromArgb(210, 210, 215);
        quitBtn.FlatStyle = FlatStyle.Flat;
        quitBtn.Font = new Font("Segoe UI Semibold", 9.5F);
        quitBtn.ForeColor = Color.FromArgb(196, 43, 28);
        quitBtn.Location = new Point(1364, 12);
        quitBtn.Name = "quitBtn";
        quitBtn.Padding = new Padding(8, 0, 8, 0);
        quitBtn.Size = new Size(96, 32);
        quitBtn.TabIndex = 2;
        quitBtn.Text = "✕  Quit";
        quitBtn.UseVisualStyleBackColor = false;
        // 
        // titleLbl
        // 
        titleLbl.AutoSize = true;
        titleLbl.Font = new Font("Segoe UI Semibold", 14F);
        titleLbl.ForeColor = Color.FromArgb(28, 28, 30);
        titleLbl.Location = new Point(16, 14);
        titleLbl.Name = "titleLbl";
        titleLbl.Size = new Size(103, 25);
        titleLbl.TabIndex = 0;
        titleLbl.Text = "TotalRecall";
        // 
        // tabs
        // 
        tabs.Controls.Add(tabCapture);
        tabs.Controls.Add(tabBrowse);
        tabs.Controls.Add(tabSettings);
        tabs.Dock = DockStyle.Fill;
        tabs.ItemSize = new Size(120, 28);
        tabs.Location = new Point(0, 56);
        tabs.Name = "tabs";
        tabs.Padding = new Point(14, 4);
        tabs.SelectedIndex = 0;
        tabs.Size = new Size(1484, 981);
        tabs.SizeMode = TabSizeMode.Fixed;
        tabs.TabIndex = 1;
        // 
        // tabCapture
        // 
        tabCapture.BackColor = Color.FromArgb(245, 245, 247);
        tabCapture.Controls.Add(capturePanel);
        tabCapture.Location = new Point(4, 32);
        tabCapture.Name = "tabCapture";
        tabCapture.Padding = new Padding(3);
        tabCapture.Size = new Size(1476, 945);
        tabCapture.TabIndex = 0;
        tabCapture.Text = "  Capture  ";
        // 
        // capturePanel
        // 
        capturePanel.BackColor = Color.FromArgb(245, 245, 247);
        capturePanel.Dock = DockStyle.Fill;
        capturePanel.Location = new Point(3, 3);
        capturePanel.Name = "capturePanel";
        capturePanel.Padding = new Padding(20);
        capturePanel.Size = new Size(1470, 939);
        capturePanel.TabIndex = 0;
        // 
        // tabBrowse
        // 
        tabBrowse.BackColor = Color.FromArgb(245, 245, 247);
        tabBrowse.Controls.Add(browsePanel);
        tabBrowse.Location = new Point(4, 32);
        tabBrowse.Name = "tabBrowse";
        tabBrowse.Padding = new Padding(3);
        tabBrowse.Size = new Size(1476, 945);
        tabBrowse.TabIndex = 1;
        tabBrowse.Text = "  Browse   ";
        // 
        // browsePanel
        // 
        browsePanel.BackColor = Color.FromArgb(245, 245, 247);
        browsePanel.Dock = DockStyle.Fill;
        browsePanel.Location = new Point(3, 3);
        browsePanel.Name = "browsePanel";
        browsePanel.Padding = new Padding(20);
        browsePanel.Size = new Size(1470, 939);
        browsePanel.TabIndex = 0;
        // 
        // tabSettings
        // 
        tabSettings.BackColor = Color.FromArgb(245, 245, 247);
        tabSettings.Controls.Add(settingsPanel);
        tabSettings.Location = new Point(4, 32);
        tabSettings.Name = "tabSettings";
        tabSettings.Padding = new Padding(3);
        tabSettings.Size = new Size(1476, 945);
        tabSettings.TabIndex = 2;
        tabSettings.Text = "  Settings ";
        // 
        // settingsPanel
        // 
        settingsPanel.AutoScroll = true;
        settingsPanel.BackColor = Color.FromArgb(245, 245, 247);
        settingsPanel.Dock = DockStyle.Fill;
        settingsPanel.Location = new Point(3, 3);
        settingsPanel.Name = "settingsPanel";
        settingsPanel.Padding = new Padding(20);
        settingsPanel.Size = new Size(1470, 939);
        settingsPanel.TabIndex = 0;
        // 
        // statusPanel
        // 
        statusPanel.BackColor = Color.FromArgb(235, 235, 238);
        statusPanel.Controls.Add(dbLbl);
        statusPanel.Controls.Add(statusLbl);
        statusPanel.Dock = DockStyle.Bottom;
        statusPanel.Location = new Point(0, 1037);
        statusPanel.Name = "statusPanel";
        statusPanel.Size = new Size(1484, 24);
        statusPanel.TabIndex = 0;
        // 
        // dbLbl
        // 
        dbLbl.Dock = DockStyle.Right;
        dbLbl.ForeColor = Color.FromArgb(102, 102, 108);
        dbLbl.Location = new Point(784, 0);
        dbLbl.Name = "dbLbl";
        dbLbl.Padding = new Padding(12, 0, 12, 0);
        dbLbl.Size = new Size(700, 24);
        dbLbl.TabIndex = 0;
        dbLbl.Text = "DB: —";
        dbLbl.TextAlign = ContentAlignment.MiddleRight;
        // 
        // statusLbl
        // 
        statusLbl.Dock = DockStyle.Left;
        statusLbl.ForeColor = Color.FromArgb(102, 102, 108);
        statusLbl.Location = new Point(0, 0);
        statusLbl.Name = "statusLbl";
        statusLbl.Padding = new Padding(12, 0, 12, 0);
        statusLbl.Size = new Size(360, 24);
        statusLbl.TabIndex = 1;
        statusLbl.Text = "Idle.";
        statusLbl.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(7F, 17F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(245, 245, 247);
        ClientSize = new Size(1484, 1061);
        Controls.Add(tabs);
        Controls.Add(statusPanel);
        Controls.Add(headerPanel);
        Font = new Font("Segoe UI", 9.5F);
        ForeColor = Color.FromArgb(28, 28, 30);
        MinimumSize = new Size(960, 600);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "TotalRecall: Private Activity Indexer";
        headerPanel.ResumeLayout(false);
        headerPanel.PerformLayout();
        tabs.ResumeLayout(false);
        tabCapture.ResumeLayout(false);
        tabBrowse.ResumeLayout(false);
        tabSettings.ResumeLayout(false);
        statusPanel.ResumeLayout(false);
        ResumeLayout(false);
    }

    private System.Windows.Forms.Panel headerPanel;
    private System.Windows.Forms.Label titleLbl;
    private System.Windows.Forms.Button quitBtn;
    private System.Windows.Forms.TabControl tabs;
    private System.Windows.Forms.TabPage tabCapture;
    private System.Windows.Forms.TabPage tabBrowse;
    private System.Windows.Forms.TabPage tabSettings;
    private TotalRecall.CapturePanel capturePanel;
    private TotalRecall.BrowsePanel browsePanel;
    private TotalRecall.SettingsPanel settingsPanel;
    private System.Windows.Forms.Panel statusPanel;
    private System.Windows.Forms.Label statusLbl;
    private System.Windows.Forms.Label dbLbl;
}
