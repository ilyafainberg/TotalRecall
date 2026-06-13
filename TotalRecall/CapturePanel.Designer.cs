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

partial class CapturePanel
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
        card = new Panel();
        cardTitleLbl = new Label();
        intervalCard = new Panel();
        intervalCaptionLbl = new Label();
        intervalLbl = new Label();
        qualityCard = new Panel();
        qualityCaptionLbl = new Label();
        qualityLbl = new Label();
        encCard = new Panel();
        encCaptionLbl = new Label();
        encLbl = new Label();
        startBtn = new Button();
        stopBtn = new Button();
        onceBtn = new Button();
        openDbBtn = new Button();
        spacer = new Panel();
        logCard = new Panel();
        logTxt = new TextBox();
        logTitleLbl = new Label();
        card.SuspendLayout();
        intervalCard.SuspendLayout();
        qualityCard.SuspendLayout();
        encCard.SuspendLayout();
        logCard.SuspendLayout();
        SuspendLayout();
        // 
        // card
        // 
        card.BackColor = Color.FromArgb(255, 255, 255);
        card.Controls.Add(cardTitleLbl);
        card.Controls.Add(intervalCard);
        card.Controls.Add(qualityCard);
        card.Controls.Add(encCard);
        card.Controls.Add(startBtn);
        card.Controls.Add(stopBtn);
        card.Controls.Add(onceBtn);
        card.Controls.Add(openDbBtn);
        card.Dock = DockStyle.Top;
        card.Location = new Point(20, 20);
        card.Name = "card";
        card.Padding = new Padding(20);
        card.Size = new Size(760, 160);
        card.TabIndex = 2;
        // 
        // cardTitleLbl
        // 
        cardTitleLbl.AutoSize = true;
        cardTitleLbl.Font = new Font("Segoe UI Semibold", 11F);
        cardTitleLbl.ForeColor = Color.FromArgb(28, 28, 30);
        cardTitleLbl.Location = new Point(20, 12);
        cardTitleLbl.Name = "cardTitleLbl";
        cardTitleLbl.Size = new Size(63, 20);
        cardTitleLbl.TabIndex = 0;
        cardTitleLbl.Text = "Capture";
        // 
        // intervalCard
        // 
        intervalCard.BackColor = Color.Transparent;
        intervalCard.Controls.Add(intervalCaptionLbl);
        intervalCard.Controls.Add(intervalLbl);
        intervalCard.Location = new Point(20, 50);
        intervalCard.Name = "intervalCard";
        intervalCard.Size = new Size(120, 40);
        intervalCard.TabIndex = 1;
        // 
        // intervalCaptionLbl
        // 
        intervalCaptionLbl.AutoSize = true;
        intervalCaptionLbl.Font = new Font("Segoe UI", 8.5F);
        intervalCaptionLbl.ForeColor = Color.FromArgb(102, 102, 108);
        intervalCaptionLbl.Location = new Point(0, 0);
        intervalCaptionLbl.Name = "intervalCaptionLbl";
        intervalCaptionLbl.Size = new Size(46, 15);
        intervalCaptionLbl.TabIndex = 0;
        intervalCaptionLbl.Text = "Interval";
        // 
        // intervalLbl
        // 
        intervalLbl.AutoSize = true;
        intervalLbl.Font = new Font("Segoe UI Semibold", 11F);
        intervalLbl.ForeColor = Color.FromArgb(28, 28, 30);
        intervalLbl.Location = new Point(0, 16);
        intervalLbl.Name = "intervalLbl";
        intervalLbl.Size = new Size(24, 20);
        intervalLbl.TabIndex = 1;
        intervalLbl.Text = "—";
        // 
        // qualityCard
        // 
        qualityCard.BackColor = Color.Transparent;
        qualityCard.Controls.Add(qualityCaptionLbl);
        qualityCard.Controls.Add(qualityLbl);
        qualityCard.Location = new Point(272, 50);
        qualityCard.Name = "qualityCard";
        qualityCard.Size = new Size(418, 40);
        qualityCard.TabIndex = 2;
        // 
        // qualityCaptionLbl
        // 
        qualityCaptionLbl.AutoSize = true;
        qualityCaptionLbl.Font = new Font("Segoe UI", 8.5F);
        qualityCaptionLbl.ForeColor = Color.FromArgb(102, 102, 108);
        qualityCaptionLbl.Location = new Point(0, 0);
        qualityCaptionLbl.Name = "qualityCaptionLbl";
        qualityCaptionLbl.Size = new Size(71, 15);
        qualityCaptionLbl.TabIndex = 0;
        qualityCaptionLbl.Text = "JPEG quality";
        // 
        // qualityLbl
        // 
        qualityLbl.AutoSize = true;
        qualityLbl.Font = new Font("Segoe UI Semibold", 11F);
        qualityLbl.ForeColor = Color.FromArgb(28, 28, 30);
        qualityLbl.Location = new Point(0, 16);
        qualityLbl.Name = "qualityLbl";
        qualityLbl.Size = new Size(24, 20);
        qualityLbl.TabIndex = 1;
        qualityLbl.Text = "—";
        // 
        // encCard
        // 
        encCard.BackColor = Color.Transparent;
        encCard.Controls.Add(encCaptionLbl);
        encCard.Controls.Add(encLbl);
        encCard.Location = new Point(146, 50);
        encCard.Name = "encCard";
        encCard.Size = new Size(120, 40);
        encCard.TabIndex = 3;
        // 
        // encCaptionLbl
        // 
        encCaptionLbl.AutoSize = true;
        encCaptionLbl.Font = new Font("Segoe UI", 8.5F);
        encCaptionLbl.ForeColor = Color.FromArgb(102, 102, 108);
        encCaptionLbl.Location = new Point(0, 0);
        encCaptionLbl.Name = "encCaptionLbl";
        encCaptionLbl.Size = new Size(64, 15);
        encCaptionLbl.TabIndex = 0;
        encCaptionLbl.Text = "Encryption";
        // 
        // encLbl
        // 
        encLbl.AutoSize = true;
        encLbl.Font = new Font("Segoe UI Semibold", 11F);
        encLbl.ForeColor = Color.FromArgb(28, 28, 30);
        encLbl.Location = new Point(0, 16);
        encLbl.Name = "encLbl";
        encLbl.Size = new Size(24, 20);
        encLbl.TabIndex = 1;
        encLbl.Text = "—";
        // 
        // startBtn
        // 
        startBtn.BackColor = Color.FromArgb(76, 154, 255);
        startBtn.Cursor = Cursors.Hand;
        startBtn.FlatAppearance.BorderColor = Color.FromArgb(210, 210, 215);
        startBtn.FlatStyle = FlatStyle.Flat;
        startBtn.Font = new Font("Segoe UI Semibold", 9.5F);
        startBtn.ForeColor = Color.White;
        startBtn.Location = new Point(20, 100);
        startBtn.Name = "startBtn";
        startBtn.Padding = new Padding(8, 0, 8, 0);
        startBtn.Size = new Size(160, 36);
        startBtn.TabIndex = 4;
        startBtn.Text = "▶  Start capture";
        startBtn.UseVisualStyleBackColor = false;
        // 
        // stopBtn
        // 
        stopBtn.BackColor = Color.FromArgb(235, 235, 238);
        stopBtn.Cursor = Cursors.Hand;
        stopBtn.Enabled = false;
        stopBtn.FlatAppearance.BorderColor = Color.FromArgb(210, 210, 215);
        stopBtn.FlatStyle = FlatStyle.Flat;
        stopBtn.Font = new Font("Segoe UI Semibold", 9.5F);
        stopBtn.ForeColor = Color.FromArgb(28, 28, 30);
        stopBtn.Location = new Point(190, 100);
        stopBtn.Name = "stopBtn";
        stopBtn.Padding = new Padding(8, 0, 8, 0);
        stopBtn.Size = new Size(100, 36);
        stopBtn.TabIndex = 5;
        stopBtn.Text = "■  Stop";
        stopBtn.UseVisualStyleBackColor = false;
        // 
        // onceBtn
        // 
        onceBtn.BackColor = Color.FromArgb(235, 235, 238);
        onceBtn.Cursor = Cursors.Hand;
        onceBtn.FlatAppearance.BorderColor = Color.FromArgb(210, 210, 215);
        onceBtn.FlatStyle = FlatStyle.Flat;
        onceBtn.Font = new Font("Segoe UI Semibold", 9.5F);
        onceBtn.ForeColor = Color.FromArgb(28, 28, 30);
        onceBtn.Location = new Point(300, 100);
        onceBtn.Name = "onceBtn";
        onceBtn.Padding = new Padding(8, 0, 8, 0);
        onceBtn.Size = new Size(150, 36);
        onceBtn.TabIndex = 6;
        onceBtn.Text = "📸  Capture now";
        onceBtn.UseVisualStyleBackColor = false;
        // 
        // openDbBtn
        // 
        openDbBtn.BackColor = Color.FromArgb(235, 235, 238);
        openDbBtn.Cursor = Cursors.Hand;
        openDbBtn.FlatAppearance.BorderColor = Color.FromArgb(210, 210, 215);
        openDbBtn.FlatStyle = FlatStyle.Flat;
        openDbBtn.Font = new Font("Segoe UI Semibold", 9.5F);
        openDbBtn.ForeColor = Color.FromArgb(28, 28, 30);
        openDbBtn.Location = new Point(460, 100);
        openDbBtn.Name = "openDbBtn";
        openDbBtn.Padding = new Padding(8, 0, 8, 0);
        openDbBtn.Size = new Size(170, 36);
        openDbBtn.TabIndex = 7;
        openDbBtn.Text = "📂  Open DB folder";
        openDbBtn.UseVisualStyleBackColor = false;
        // 
        // spacer
        // 
        spacer.BackColor = Color.FromArgb(245, 245, 247);
        spacer.Dock = DockStyle.Top;
        spacer.Location = new Point(20, 180);
        spacer.Name = "spacer";
        spacer.Size = new Size(760, 16);
        spacer.TabIndex = 1;
        // 
        // logCard
        // 
        logCard.BackColor = Color.FromArgb(255, 255, 255);
        logCard.Controls.Add(logTxt);
        logCard.Controls.Add(logTitleLbl);
        logCard.Dock = DockStyle.Fill;
        logCard.Location = new Point(20, 196);
        logCard.Name = "logCard";
        logCard.Padding = new Padding(20, 16, 20, 20);
        logCard.Size = new Size(760, 384);
        logCard.TabIndex = 0;
        // 
        // logTxt
        // 
        logTxt.BackColor = Color.FromArgb(245, 245, 247);
        logTxt.BorderStyle = BorderStyle.None;
        logTxt.Dock = DockStyle.Fill;
        logTxt.Font = new Font("Cascadia Mono", 9.5F);
        logTxt.ForeColor = Color.FromArgb(28, 28, 30);
        logTxt.Location = new Point(20, 44);
        logTxt.Multiline = true;
        logTxt.Name = "logTxt";
        logTxt.ReadOnly = true;
        logTxt.ScrollBars = ScrollBars.Vertical;
        logTxt.Size = new Size(720, 320);
        logTxt.TabIndex = 0;
        // 
        // logTitleLbl
        // 
        logTitleLbl.Dock = DockStyle.Top;
        logTitleLbl.Font = new Font("Segoe UI Semibold", 11F);
        logTitleLbl.ForeColor = Color.FromArgb(28, 28, 30);
        logTitleLbl.Location = new Point(20, 16);
        logTitleLbl.Name = "logTitleLbl";
        logTitleLbl.Size = new Size(720, 28);
        logTitleLbl.TabIndex = 1;
        logTitleLbl.Text = "Activity log";
        // 
        // CapturePanel
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(245, 245, 247);
        Controls.Add(logCard);
        Controls.Add(spacer);
        Controls.Add(card);
        Name = "CapturePanel";
        Padding = new Padding(20);
        Size = new Size(800, 600);
        card.ResumeLayout(false);
        card.PerformLayout();
        intervalCard.ResumeLayout(false);
        intervalCard.PerformLayout();
        qualityCard.ResumeLayout(false);
        qualityCard.PerformLayout();
        encCard.ResumeLayout(false);
        encCard.PerformLayout();
        logCard.ResumeLayout(false);
        logCard.PerformLayout();
        ResumeLayout(false);
    }

    private System.Windows.Forms.Panel card;
    private System.Windows.Forms.Label cardTitleLbl;
    private System.Windows.Forms.Panel intervalCard;
    private System.Windows.Forms.Label intervalCaptionLbl;
    private System.Windows.Forms.Label intervalLbl;
    private System.Windows.Forms.Panel qualityCard;
    private System.Windows.Forms.Label qualityCaptionLbl;
    private System.Windows.Forms.Label qualityLbl;
    private System.Windows.Forms.Panel encCard;
    private System.Windows.Forms.Label encCaptionLbl;
    private System.Windows.Forms.Label encLbl;
    private System.Windows.Forms.Button startBtn;
    private System.Windows.Forms.Button stopBtn;
    private System.Windows.Forms.Button onceBtn;
    private System.Windows.Forms.Button openDbBtn;
    private System.Windows.Forms.Panel spacer;
    private System.Windows.Forms.Panel logCard;
    private System.Windows.Forms.TextBox logTxt;
    private System.Windows.Forms.Label logTitleLbl;
}
