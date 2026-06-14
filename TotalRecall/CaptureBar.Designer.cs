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

partial class CaptureBar
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        dotLbl = new Label();
        statusLbl = new Label();
        statsLbl = new Label();
        lastLbl = new Label();
        startBtn = new Button();
        stopBtn = new Button();
        SuspendLayout();
        // 
        // dotLbl
        // 
        dotLbl.AutoSize = true;
        dotLbl.Font = new Font("Segoe UI Symbol", 14F);
        dotLbl.ForeColor = Color.FromArgb(170, 170, 178);
        dotLbl.Location = new Point(8, 6);
        dotLbl.Name = "dotLbl";
        dotLbl.Size = new Size(20, 25);
        dotLbl.TabIndex = 0;
        dotLbl.Text = "●";
        // 
        // statusLbl
        // 
        statusLbl.AutoSize = true;
        statusLbl.Font = new Font("Segoe UI Semibold", 10.5F);
        statusLbl.ForeColor = Color.FromArgb(102, 102, 108);
        statusLbl.Location = new Point(28, 9);
        statusLbl.Name = "statusLbl";
        statusLbl.Size = new Size(36, 19);
        statusLbl.TabIndex = 1;
        statusLbl.Text = "Idle";
        // 
        // statsLbl
        // 
        statsLbl.AutoSize = true;
        statsLbl.Font = new Font("Segoe UI", 9F);
        statsLbl.ForeColor = Color.FromArgb(102, 102, 108);
        statsLbl.Location = new Point(12, 36);
        statsLbl.Name = "statsLbl";
        statsLbl.Size = new Size(14, 15);
        statsLbl.TabIndex = 2;
        statsLbl.Text = "—";
        // 
        // lastLbl
        // 
        lastLbl.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        lastLbl.AutoSize = true;
        lastLbl.Font = new Font("Segoe UI", 9F);
        lastLbl.ForeColor = Color.FromArgb(102, 102, 108);
        lastLbl.Location = new Point(560, 36);
        lastLbl.Name = "lastLbl";
        lastLbl.Size = new Size(38, 15);
        lastLbl.TabIndex = 3;
        lastLbl.Text = "Last: —";
        lastLbl.TextAlign = ContentAlignment.MiddleRight;
        // 
        // startBtn
        // 
        startBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        startBtn.BackColor = Color.FromArgb(76, 154, 255);
        startBtn.Cursor = Cursors.Hand;
        startBtn.FlatAppearance.BorderColor = Color.FromArgb(210, 210, 215);
        startBtn.FlatStyle = FlatStyle.Flat;
        startBtn.Font = new Font("Segoe UI Semibold", 9.5F);
        startBtn.ForeColor = Color.White;
        startBtn.Location = new Point(560, 6);
        startBtn.Name = "startBtn";
        startBtn.Padding = new Padding(8, 0, 8, 0);
        startBtn.Size = new Size(96, 28);
        startBtn.TabIndex = 4;
        startBtn.Text = "▶  Start";
        startBtn.UseVisualStyleBackColor = false;
        // 
        // stopBtn
        // 
        stopBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        stopBtn.BackColor = Color.FromArgb(235, 235, 238);
        stopBtn.Cursor = Cursors.Hand;
        stopBtn.FlatAppearance.BorderColor = Color.FromArgb(210, 210, 215);
        stopBtn.FlatStyle = FlatStyle.Flat;
        stopBtn.Font = new Font("Segoe UI Semibold", 9.5F);
        stopBtn.ForeColor = Color.FromArgb(28, 28, 30);
        stopBtn.Location = new Point(660, 6);
        stopBtn.Name = "stopBtn";
        stopBtn.Padding = new Padding(8, 0, 8, 0);
        stopBtn.Size = new Size(80, 28);
        stopBtn.TabIndex = 5;
        stopBtn.Text = "■  Stop";
        stopBtn.UseVisualStyleBackColor = false;
        // 
        // CaptureBar
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(235, 235, 238);
        Controls.Add(stopBtn);
        Controls.Add(startBtn);
        Controls.Add(lastLbl);
        Controls.Add(statsLbl);
        Controls.Add(statusLbl);
        Controls.Add(dotLbl);
        Name = "CaptureBar";
        Size = new Size(760, 56);
        ResumeLayout(false);
        PerformLayout();
    }

    private System.Windows.Forms.Label dotLbl;
    private System.Windows.Forms.Label statusLbl;
    private System.Windows.Forms.Label statsLbl;
    private System.Windows.Forms.Label lastLbl;
    private System.Windows.Forms.Button startBtn;
    private System.Windows.Forms.Button stopBtn;
}
