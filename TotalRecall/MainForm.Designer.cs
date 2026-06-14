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
        titleLbl = new Label();
        captureBar = new CaptureBar();
        menuBtn = new Button();
        quitBtn = new Button();
        browsePanel = new BrowsePanel();
        statusPanel = new Panel();
        dbLbl = new Label();
        statusLbl = new Label();
        headerPanel.SuspendLayout();
        statusPanel.SuspendLayout();
        SuspendLayout();
        // 
        // headerPanel
        // 
        headerPanel.BackColor = Color.FromArgb(235, 235, 238);
        headerPanel.Controls.Add(captureBar);
        headerPanel.Controls.Add(titleLbl);
        headerPanel.Controls.Add(menuBtn);
        headerPanel.Controls.Add(quitBtn);
        headerPanel.Dock = DockStyle.Top;
        headerPanel.Location = new Point(0, 0);
        headerPanel.Name = "headerPanel";
        headerPanel.Padding = new Padding(16, 8, 16, 8);
        headerPanel.Size = new Size(1484, 72);
        headerPanel.TabIndex = 2;
        // 
        // titleLbl
        // 
        titleLbl.AutoSize = true;
        titleLbl.Font = new Font("Segoe UI Semibold", 13F);
        titleLbl.ForeColor = Color.FromArgb(28, 28, 30);
        titleLbl.Location = new Point(16, 22);
        titleLbl.Name = "titleLbl";
        titleLbl.Size = new Size(98, 24);
        titleLbl.TabIndex = 0;
        titleLbl.Text = "TotalRecall";
        // 
        // captureBar
        // 
        captureBar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        captureBar.BackColor = Color.FromArgb(235, 235, 238);
        captureBar.Location = new Point(150, 8);
        captureBar.Name = "captureBar";
        captureBar.Size = new Size(1140, 56);
        captureBar.TabIndex = 1;
        // 
        // menuBtn
        // 
        menuBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        menuBtn.BackColor = Color.FromArgb(245, 245, 247);
        menuBtn.Cursor = Cursors.Hand;
        menuBtn.FlatAppearance.BorderColor = Color.FromArgb(210, 210, 215);
        menuBtn.FlatStyle = FlatStyle.Flat;
        menuBtn.Font = new Font("Segoe UI Symbol", 12F);
        menuBtn.ForeColor = Color.FromArgb(28, 28, 30);
        menuBtn.Location = new Point(1300, 18);
        menuBtn.Name = "menuBtn";
        menuBtn.Padding = new Padding(0);
        menuBtn.Size = new Size(40, 36);
        menuBtn.TabIndex = 2;
        menuBtn.Text = "☰";
        menuBtn.UseVisualStyleBackColor = false;
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
        quitBtn.Location = new Point(1352, 18);
        quitBtn.Name = "quitBtn";
        quitBtn.Padding = new Padding(8, 0, 8, 0);
        quitBtn.Size = new Size(96, 36);
        quitBtn.TabIndex = 3;
        quitBtn.Text = "✕  Quit";
        quitBtn.UseVisualStyleBackColor = false;
        // 
        // browsePanel
        // 
        browsePanel.BackColor = Color.FromArgb(245, 245, 247);
        browsePanel.Dock = DockStyle.Fill;
        browsePanel.Location = new Point(0, 72);
        browsePanel.Name = "browsePanel";
        browsePanel.Padding = new Padding(20);
        browsePanel.Size = new Size(1484, 965);
        browsePanel.TabIndex = 0;
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
        statusLbl.Size = new Size(420, 24);
        statusLbl.TabIndex = 1;
        statusLbl.Text = "";
        statusLbl.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(7F, 17F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(245, 245, 247);
        ClientSize = new Size(1484, 1061);
        Controls.Add(browsePanel);
        Controls.Add(statusPanel);
        Controls.Add(headerPanel);
        Font = new Font("Segoe UI", 9.5F);
        ForeColor = Color.FromArgb(28, 28, 30);
        KeyPreview = true;
        MinimumSize = new Size(1040, 680);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "TotalRecall: Private Activity Indexer";
        headerPanel.ResumeLayout(false);
        headerPanel.PerformLayout();
        statusPanel.ResumeLayout(false);
        ResumeLayout(false);
    }

    private System.Windows.Forms.Panel headerPanel;
    private System.Windows.Forms.Label titleLbl;
    private TotalRecall.CaptureBar captureBar;
    private System.Windows.Forms.Button menuBtn;
    private System.Windows.Forms.Button quitBtn;
    private TotalRecall.BrowsePanel browsePanel;
    private System.Windows.Forms.Panel statusPanel;
    private System.Windows.Forms.Label statusLbl;
    private System.Windows.Forms.Label dbLbl;
}
