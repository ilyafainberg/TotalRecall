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
        menuBtn = new Button();
        capStartBtn = new Button();
        capLastLbl = new Label();
        capInfoLbl = new Label();
        capStateLbl = new Label();
        capDotLbl = new Label();
        titleLbl = new Label();
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
        headerPanel.Controls.Add(menuBtn);
        headerPanel.Controls.Add(capStartBtn);
        headerPanel.Controls.Add(capLastLbl);
        headerPanel.Controls.Add(capInfoLbl);
        headerPanel.Controls.Add(capStateLbl);
        headerPanel.Controls.Add(capDotLbl);
        headerPanel.Controls.Add(titleLbl);
        headerPanel.Dock = DockStyle.Top;
        headerPanel.Location = new Point(0, 0);
        headerPanel.Name = "headerPanel";
        headerPanel.Padding = new Padding(16, 8, 16, 8);
        headerPanel.Size = new Size(1484, 72);
        headerPanel.TabIndex = 2;
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
        menuBtn.Location = new Point(1426, 15);
        menuBtn.Name = "menuBtn";
        menuBtn.Size = new Size(40, 32);
        menuBtn.TabIndex = 2;
        menuBtn.Text = "☰";
        menuBtn.UseVisualStyleBackColor = false;
        // 
        // capStartBtn
        // 
        capStartBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        capStartBtn.BackColor = Color.FromArgb(76, 154, 255);
        capStartBtn.Cursor = Cursors.Hand;
        capStartBtn.FlatAppearance.BorderColor = Color.FromArgb(210, 210, 215);
        capStartBtn.FlatStyle = FlatStyle.Flat;
        capStartBtn.Font = new Font("Segoe UI Semibold", 9.5F);
        capStartBtn.ForeColor = Color.White;
        capStartBtn.Location = new Point(1306, 15);
        capStartBtn.Name = "capStartBtn";
        capStartBtn.Padding = new Padding(8, 0, 8, 0);
        capStartBtn.Size = new Size(105, 32);
        capStartBtn.TabIndex = 8;
        capStartBtn.Text = "▶  Start";
        capStartBtn.UseVisualStyleBackColor = false;
        // 
        // capLastLbl
        // 
        capLastLbl.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        capLastLbl.Font = new Font("Segoe UI", 9F);
        capLastLbl.ForeColor = Color.FromArgb(102, 102, 108);
        capLastLbl.Location = new Point(1186, 50);
        capLastLbl.Name = "capLastLbl";
        capLastLbl.Size = new Size(280, 19);
        capLastLbl.TabIndex = 7;
        capLastLbl.Text = "Last: —";
        capLastLbl.TextAlign = ContentAlignment.MiddleRight;
        // 
        // capInfoLbl
        // 
        capInfoLbl.AutoSize = true;
        capInfoLbl.Font = new Font("Segoe UI", 9F);
        capInfoLbl.ForeColor = Color.FromArgb(102, 102, 108);
        capInfoLbl.Location = new Point(164, 44);
        capInfoLbl.Name = "capInfoLbl";
        capInfoLbl.Size = new Size(19, 15);
        capInfoLbl.TabIndex = 6;
        capInfoLbl.Text = "—";
        // 
        // capStateLbl
        // 
        capStateLbl.AutoSize = true;
        capStateLbl.Font = new Font("Segoe UI Semibold", 10.5F);
        capStateLbl.ForeColor = Color.FromArgb(102, 102, 108);
        capStateLbl.Location = new Point(184, 17);
        capStateLbl.Name = "capStateLbl";
        capStateLbl.Size = new Size(32, 19);
        capStateLbl.TabIndex = 5;
        capStateLbl.Text = "Idle";
        // 
        // capDotLbl
        // 
        capDotLbl.AutoSize = true;
        capDotLbl.Font = new Font("Segoe UI Symbol", 14F);
        capDotLbl.ForeColor = Color.FromArgb(170, 170, 178);
        capDotLbl.Location = new Point(160, 14);
        capDotLbl.Name = "capDotLbl";
        capDotLbl.Size = new Size(28, 25);
        capDotLbl.TabIndex = 4;
        capDotLbl.Text = "●";
        // 
        // titleLbl
        // 
        titleLbl.AutoSize = true;
        titleLbl.Font = new Font("Segoe UI Semibold", 13F);
        titleLbl.ForeColor = Color.FromArgb(28, 28, 30);
        titleLbl.Location = new Point(16, 22);
        titleLbl.Name = "titleLbl";
        titleLbl.Size = new Size(100, 25);
        titleLbl.TabIndex = 0;
        titleLbl.Text = "TotalRecall";
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
    private System.Windows.Forms.Label capDotLbl;
    private System.Windows.Forms.Label capStateLbl;
    private System.Windows.Forms.Label capInfoLbl;
    private System.Windows.Forms.Label capLastLbl;
    private System.Windows.Forms.Button capStartBtn;
    private System.Windows.Forms.Button menuBtn;
    private TotalRecall.BrowsePanel browsePanel;
    private System.Windows.Forms.Panel statusPanel;
    private System.Windows.Forms.Label statusLbl;
    private System.Windows.Forms.Label dbLbl;
}
