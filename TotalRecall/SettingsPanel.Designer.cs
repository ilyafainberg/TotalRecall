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

partial class SettingsPanel
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
        panel1 = new Panel();
        titleLbl = new Label();
        intervalCaptionLbl = new Label();
        intervalNud = new NumericUpDown();
        qualityCaptionLbl = new Label();
        qualityBar = new TrackBar();
        qualityValueLbl = new Label();
        storeImagesChk = new CheckBox();
        performanceCaptionLbl = new Label();
        changeDetectionChk = new CheckBox();
        foregroundOnlyChk = new CheckBox();
        excludedAppsCaptionLbl = new Label();
        excludedAppsTxt = new TextBox();
        ocrMaxDimensionCaptionLbl = new Label();
        ocrMaxDimensionNud = new NumericUpDown();
        dbPathCaptionLbl = new Label();
        dbPathTxt = new TextBox();
        dbBrowseBtn = new Button();
        langCaptionLbl = new Label();
        langCombo = new ComboBox();
        encCaptionLbl = new Label();
        encNoneRb = new RadioButton();
        encUserRb = new RadioButton();
        encPassRb = new RadioButton();
        passTxt = new TextBox();
        hintLbl = new Label();
        behaviorCaptionLbl = new Label();
        startAtLoginChk = new CheckBox();
        minToTrayChk = new CheckBox();
        retentionCaptionLbl = new Label();
        purgeImagesChk = new CheckBox();
        purgeImagesNud = new NumericUpDown();
        purgeImagesDaysLbl = new Label();
        purgeAllChk = new CheckBox();
        purgeAllNud = new NumericUpDown();
        purgeAllDaysLbl = new Label();
        compactAfterRetentionChk = new CheckBox();
        compactAfterRetentionHoursNud = new NumericUpDown();
        compactAfterRetentionHoursLbl = new Label();
        purgeNowBtn = new Button();
        clearDbBtn = new Button();
        saveBtn = new Button();
        cancelBtn = new Button();
        card.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)intervalNud).BeginInit();
        ((System.ComponentModel.ISupportInitialize)qualityBar).BeginInit();
        ((System.ComponentModel.ISupportInitialize)ocrMaxDimensionNud).BeginInit();
        ((System.ComponentModel.ISupportInitialize)purgeImagesNud).BeginInit();
        ((System.ComponentModel.ISupportInitialize)purgeAllNud).BeginInit();
        ((System.ComponentModel.ISupportInitialize)compactAfterRetentionHoursNud).BeginInit();
        SuspendLayout();
        // 
        // card
        // 
        card.BackColor = Color.FromArgb(255, 255, 255);
        card.Controls.Add(panel1);
        card.Controls.Add(titleLbl);
        card.Controls.Add(intervalCaptionLbl);
        card.Controls.Add(intervalNud);
        card.Controls.Add(qualityCaptionLbl);
        card.Controls.Add(qualityBar);
        card.Controls.Add(qualityValueLbl);
        card.Controls.Add(storeImagesChk);
        card.Controls.Add(performanceCaptionLbl);
        card.Controls.Add(changeDetectionChk);
        card.Controls.Add(foregroundOnlyChk);
        card.Controls.Add(excludedAppsCaptionLbl);
        card.Controls.Add(excludedAppsTxt);
        card.Controls.Add(ocrMaxDimensionCaptionLbl);
        card.Controls.Add(ocrMaxDimensionNud);
        card.Controls.Add(dbPathCaptionLbl);
        card.Controls.Add(dbPathTxt);
        card.Controls.Add(dbBrowseBtn);
        card.Controls.Add(langCaptionLbl);
        card.Controls.Add(langCombo);
        card.Controls.Add(encCaptionLbl);
        card.Controls.Add(encNoneRb);
        card.Controls.Add(encUserRb);
        card.Controls.Add(encPassRb);
        card.Controls.Add(passTxt);
        card.Controls.Add(hintLbl);
        card.Controls.Add(behaviorCaptionLbl);
        card.Controls.Add(startAtLoginChk);
        card.Controls.Add(minToTrayChk);
        card.Controls.Add(retentionCaptionLbl);
        card.Controls.Add(purgeImagesChk);
        card.Controls.Add(purgeImagesNud);
        card.Controls.Add(purgeImagesDaysLbl);
        card.Controls.Add(purgeAllChk);
        card.Controls.Add(purgeAllNud);
        card.Controls.Add(purgeAllDaysLbl);
        card.Controls.Add(compactAfterRetentionChk);
        card.Controls.Add(compactAfterRetentionHoursNud);
        card.Controls.Add(compactAfterRetentionHoursLbl);
        card.Controls.Add(purgeNowBtn);
        card.Controls.Add(clearDbBtn);
        card.Controls.Add(saveBtn);
        card.Controls.Add(cancelBtn);
        card.Dock = DockStyle.Top;
        card.Location = new Point(20, 20);
        card.Name = "card";
        card.Padding = new Padding(24, 20, 24, 24);
        card.Size = new Size(743, 1170);
        card.TabIndex = 0;
        // 
        // panel1
        // 
        panel1.BackColor = Color.FromArgb(224, 224, 224);
        panel1.Location = new Point(24, 1109);
        panel1.Name = "panel1";
        panel1.Size = new Size(715, 2);
        panel1.TabIndex = 30;
        // 
        // titleLbl
        // 
        titleLbl.AutoSize = true;
        titleLbl.Font = new Font("Segoe UI Semibold", 12F);
        titleLbl.ForeColor = Color.FromArgb(28, 28, 30);
        titleLbl.Location = new Point(24, 16);
        titleLbl.Name = "titleLbl";
        titleLbl.Size = new Size(70, 21);
        titleLbl.TabIndex = 0;
        titleLbl.Text = "Settings";
        // 
        // intervalCaptionLbl
        // 
        intervalCaptionLbl.AutoSize = true;
        intervalCaptionLbl.Font = new Font("Segoe UI Semibold", 9.5F);
        intervalCaptionLbl.ForeColor = Color.FromArgb(28, 28, 30);
        intervalCaptionLbl.Location = new Point(24, 60);
        intervalCaptionLbl.Name = "intervalCaptionLbl";
        intervalCaptionLbl.Size = new Size(167, 17);
        intervalCaptionLbl.TabIndex = 1;
        intervalCaptionLbl.Text = "Capture interval (seconds)";
        // 
        // intervalNud
        // 
        intervalNud.Location = new Point(24, 84);
        intervalNud.Maximum = new decimal(new int[] { 3600, 0, 0, 0 });
        intervalNud.Minimum = new decimal(new int[] { 2, 0, 0, 0 });
        intervalNud.Name = "intervalNud";
        intervalNud.Size = new Size(120, 23);
        intervalNud.TabIndex = 2;
        intervalNud.Value = new decimal(new int[] { 10, 0, 0, 0 });
        // 
        // qualityCaptionLbl
        // 
        qualityCaptionLbl.AutoSize = true;
        qualityCaptionLbl.Font = new Font("Segoe UI Semibold", 9.5F);
        qualityCaptionLbl.ForeColor = Color.FromArgb(28, 28, 30);
        qualityCaptionLbl.Location = new Point(24, 130);
        qualityCaptionLbl.Name = "qualityCaptionLbl";
        qualityCaptionLbl.Size = new Size(289, 17);
        qualityCaptionLbl.TabIndex = 3;
        qualityCaptionLbl.Text = "JPEG quality (30 = small / 95 = large, readable)";
        // 
        // qualityBar
        // 
        qualityBar.Location = new Point(24, 150);
        qualityBar.Maximum = 95;
        qualityBar.Minimum = 30;
        qualityBar.Name = "qualityBar";
        qualityBar.Size = new Size(480, 45);
        qualityBar.TabIndex = 4;
        qualityBar.TickFrequency = 5;
        qualityBar.Value = 75;
        // 
        // qualityValueLbl
        // 
        qualityValueLbl.AutoSize = true;
        qualityValueLbl.Font = new Font("Segoe UI Semibold", 11F);
        qualityValueLbl.ForeColor = Color.FromArgb(28, 28, 30);
        qualityValueLbl.Location = new Point(516, 158);
        qualityValueLbl.Name = "qualityValueLbl";
        qualityValueLbl.Size = new Size(25, 20);
        qualityValueLbl.TabIndex = 5;
        qualityValueLbl.Text = "75";
        // 
        // storeImagesChk
        // 
        storeImagesChk.AutoSize = true;
        storeImagesChk.Checked = true;
        storeImagesChk.CheckState = CheckState.Checked;
        storeImagesChk.Font = new Font("Segoe UI", 9.5F);
        storeImagesChk.ForeColor = Color.FromArgb(28, 28, 30);
        storeImagesChk.Location = new Point(24, 202);
        storeImagesChk.Name = "storeImagesChk";
        storeImagesChk.Size = new Size(403, 21);
        storeImagesChk.TabIndex = 6;
        storeImagesChk.Text = "Store JPEGs in DB (uncheck to keep OCR text only, much smaller)";
        // 
        // performanceCaptionLbl
        // 
        performanceCaptionLbl.AutoSize = true;
        performanceCaptionLbl.Font = new Font("Segoe UI Semibold", 9.5F);
        performanceCaptionLbl.ForeColor = Color.FromArgb(28, 28, 30);
        performanceCaptionLbl.Location = new Point(24, 238);
        performanceCaptionLbl.Name = "performanceCaptionLbl";
        performanceCaptionLbl.Size = new Size(85, 17);
        performanceCaptionLbl.TabIndex = 31;
        performanceCaptionLbl.Text = "Performance";
        // 
        // changeDetectionChk
        // 
        changeDetectionChk.AutoSize = true;
        changeDetectionChk.Checked = true;
        changeDetectionChk.CheckState = CheckState.Checked;
        changeDetectionChk.Font = new Font("Segoe UI", 9.5F);
        changeDetectionChk.ForeColor = Color.FromArgb(28, 28, 30);
        changeDetectionChk.Location = new Point(24, 262);
        changeDetectionChk.Name = "changeDetectionChk";
        changeDetectionChk.Size = new Size(374, 21);
        changeDetectionChk.TabIndex = 32;
        changeDetectionChk.Text = "Skip unchanged windows (avoids repeated OCR and writes)";
        // 
        // foregroundOnlyChk
        // 
        foregroundOnlyChk.AutoSize = true;
        foregroundOnlyChk.Font = new Font("Segoe UI", 9.5F);
        foregroundOnlyChk.ForeColor = Color.FromArgb(28, 28, 30);
        foregroundOnlyChk.Location = new Point(24, 288);
        foregroundOnlyChk.Name = "foregroundOnlyChk";
        foregroundOnlyChk.Size = new Size(220, 21);
        foregroundOnlyChk.TabIndex = 33;
        foregroundOnlyChk.Text = "Capture foreground window only";
        // 
        // excludedAppsCaptionLbl
        // 
        excludedAppsCaptionLbl.AutoSize = true;
        excludedAppsCaptionLbl.Font = new Font("Segoe UI Semibold", 9.5F);
        excludedAppsCaptionLbl.ForeColor = Color.FromArgb(28, 28, 30);
        excludedAppsCaptionLbl.Location = new Point(24, 320);
        excludedAppsCaptionLbl.Name = "excludedAppsCaptionLbl";
        excludedAppsCaptionLbl.Size = new Size(419, 17);
        excludedAppsCaptionLbl.TabIndex = 34;
        excludedAppsCaptionLbl.Text = "Exclude apps/processes/titles (comma, semicolon, or line separated)";
        // 
        // excludedAppsTxt
        // 
        excludedAppsTxt.BackColor = Color.FromArgb(245, 245, 247);
        excludedAppsTxt.BorderStyle = BorderStyle.FixedSingle;
        excludedAppsTxt.ForeColor = Color.FromArgb(28, 28, 30);
        excludedAppsTxt.Location = new Point(24, 342);
        excludedAppsTxt.Multiline = true;
        excludedAppsTxt.Name = "excludedAppsTxt";
        excludedAppsTxt.PlaceholderText = "1Password, KeePass, banking";
        excludedAppsTxt.ScrollBars = ScrollBars.Vertical;
        excludedAppsTxt.Size = new Size(480, 52);
        excludedAppsTxt.TabIndex = 35;
        // 
        // ocrMaxDimensionCaptionLbl
        // 
        ocrMaxDimensionCaptionLbl.AutoSize = true;
        ocrMaxDimensionCaptionLbl.Font = new Font("Segoe UI Semibold", 9.5F);
        ocrMaxDimensionCaptionLbl.ForeColor = Color.FromArgb(28, 28, 30);
        ocrMaxDimensionCaptionLbl.Location = new Point(24, 408);
        ocrMaxDimensionCaptionLbl.Name = "ocrMaxDimensionCaptionLbl";
        ocrMaxDimensionCaptionLbl.Size = new Size(370, 17);
        ocrMaxDimensionCaptionLbl.TabIndex = 36;
        ocrMaxDimensionCaptionLbl.Text = "OCR max image dimension (lower = less CPU, less accuracy)";
        // 
        // ocrMaxDimensionNud
        // 
        ocrMaxDimensionNud.Increment = new decimal(new int[] { 100, 0, 0, 0 });
        ocrMaxDimensionNud.Location = new Point(24, 432);
        ocrMaxDimensionNud.Maximum = new decimal(new int[] { 3840, 0, 0, 0 });
        ocrMaxDimensionNud.Minimum = new decimal(new int[] { 400, 0, 0, 0 });
        ocrMaxDimensionNud.Name = "ocrMaxDimensionNud";
        ocrMaxDimensionNud.Size = new Size(120, 23);
        ocrMaxDimensionNud.TabIndex = 37;
        ocrMaxDimensionNud.Value = new decimal(new int[] { 1600, 0, 0, 0 });
        // 
        // dbPathCaptionLbl
        // 
        dbPathCaptionLbl.AutoSize = true;
        dbPathCaptionLbl.Font = new Font("Segoe UI Semibold", 9.5F);
        dbPathCaptionLbl.ForeColor = Color.FromArgb(28, 28, 30);
        dbPathCaptionLbl.Location = new Point(24, 478);
        dbPathCaptionLbl.Name = "dbPathCaptionLbl";
        dbPathCaptionLbl.Size = new Size(85, 17);
        dbPathCaptionLbl.TabIndex = 7;
        dbPathCaptionLbl.Text = "Database file";
        // 
        // dbPathTxt
        // 
        dbPathTxt.BackColor = Color.FromArgb(245, 245, 247);
        dbPathTxt.BorderStyle = BorderStyle.FixedSingle;
        dbPathTxt.ForeColor = Color.FromArgb(28, 28, 30);
        dbPathTxt.Location = new Point(24, 502);
        dbPathTxt.Name = "dbPathTxt";
        dbPathTxt.Size = new Size(480, 23);
        dbPathTxt.TabIndex = 8;
        dbPathTxt.TextChanged += dbPathTxt_TextChanged;
        // 
        // dbBrowseBtn
        // 
        dbBrowseBtn.BackColor = Color.FromArgb(235, 235, 238);
        dbBrowseBtn.Cursor = Cursors.Hand;
        dbBrowseBtn.FlatAppearance.BorderColor = Color.FromArgb(210, 210, 215);
        dbBrowseBtn.FlatStyle = FlatStyle.Flat;
        dbBrowseBtn.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
        dbBrowseBtn.ForeColor = Color.FromArgb(28, 28, 30);
        dbBrowseBtn.Location = new Point(510, 502);
        dbBrowseBtn.Name = "dbBrowseBtn";
        dbBrowseBtn.Padding = new Padding(8, 0, 8, 0);
        dbBrowseBtn.Size = new Size(53, 23);
        dbBrowseBtn.TabIndex = 9;
        dbBrowseBtn.Text = "۰۰۰";
        dbBrowseBtn.UseVisualStyleBackColor = false;
        // 
        // langCaptionLbl
        // 
        langCaptionLbl.AutoSize = true;
        langCaptionLbl.Font = new Font("Segoe UI Semibold", 9.5F);
        langCaptionLbl.ForeColor = Color.FromArgb(28, 28, 30);
        langCaptionLbl.Location = new Point(24, 548);
        langCaptionLbl.Name = "langCaptionLbl";
        langCaptionLbl.Size = new Size(366, 17);
        langCaptionLbl.TabIndex = 10;
        langCaptionLbl.Text = "OCR language (must have <lang>.traineddata in tessdata/)";
        // 
        // langCombo
        // 
        langCombo.BackColor = Color.FromArgb(245, 245, 247);
        langCombo.FlatStyle = FlatStyle.Flat;
        langCombo.ForeColor = Color.FromArgb(28, 28, 30);
        langCombo.Items.AddRange(new object[] { "eng", "eng+fra", "eng+deu", "fra", "deu", "spa", "ita", "nld", "por", "rus", "jpn", "kor", "chi_sim", "chi_tra" });
        langCombo.Location = new Point(24, 572);
        langCombo.Name = "langCombo";
        langCombo.Size = new Size(200, 23);
        langCombo.TabIndex = 11;
        // 
        // encCaptionLbl
        // 
        encCaptionLbl.AutoSize = true;
        encCaptionLbl.Font = new Font("Segoe UI Semibold", 9.5F);
        encCaptionLbl.ForeColor = Color.FromArgb(28, 28, 30);
        encCaptionLbl.Location = new Point(24, 618);
        encCaptionLbl.Name = "encCaptionLbl";
        encCaptionLbl.Size = new Size(134, 17);
        encCaptionLbl.TabIndex = 12;
        encCaptionLbl.Text = "Database encryption";
        // 
        // encNoneRb
        // 
        encNoneRb.AutoSize = true;
        encNoneRb.Font = new Font("Segoe UI", 9.5F);
        encNoneRb.ForeColor = Color.FromArgb(28, 28, 30);
        encNoneRb.Location = new Point(24, 642);
        encNoneRb.Name = "encNoneRb";
        encNoneRb.Size = new Size(122, 21);
        encNoneRb.TabIndex = 13;
        encNoneRb.Text = "None (unsecure)";
        // 
        // encUserRb
        // 
        encUserRb.AutoSize = true;
        encUserRb.Font = new Font("Segoe UI", 9.5F);
        encUserRb.ForeColor = Color.FromArgb(28, 28, 30);
        encUserRb.Location = new Point(24, 668);
        encUserRb.Name = "encUserRb";
        encUserRb.Size = new Size(428, 21);
        encUserRb.TabIndex = 14;
        encUserRb.Text = "Windows account (DPAPI-protected key, auto-unlock for current user)";
        // 
        // encPassRb
        // 
        encPassRb.AutoSize = true;
        encPassRb.Font = new Font("Segoe UI", 9.5F);
        encPassRb.ForeColor = Color.FromArgb(28, 28, 30);
        encPassRb.Location = new Point(24, 694);
        encPassRb.Name = "encPassRb";
        encPassRb.Size = new Size(334, 21);
        encPassRb.TabIndex = 15;
        encPassRb.Text = "Passphrase (you'll be prompted on each app launch)";
        // 
        // passTxt
        // 
        passTxt.BackColor = Color.FromArgb(245, 245, 247);
        passTxt.BorderStyle = BorderStyle.FixedSingle;
        passTxt.Enabled = false;
        passTxt.ForeColor = Color.FromArgb(28, 28, 30);
        passTxt.Location = new Point(48, 720);
        passTxt.Name = "passTxt";
        passTxt.PlaceholderText = "Enter passphrase…";
        passTxt.Size = new Size(360, 23);
        passTxt.TabIndex = 16;
        passTxt.UseSystemPasswordChar = true;
        // 
        // hintLbl
        // 
        hintLbl.Font = new Font("Segoe UI", 9.5F);
        hintLbl.ForeColor = Color.FromArgb(102, 102, 108);
        hintLbl.Location = new Point(24, 758);
        hintLbl.Name = "hintLbl";
        hintLbl.Size = new Size(590, 60);
        hintLbl.TabIndex = 17;
        hintLbl.Text = "Note: changing encryption mode or passphrase only affects newly-created databases. To re-encrypt an existing DB, point to a new file path and re-capture.";
        // 
        // behaviorCaptionLbl
        // 
        behaviorCaptionLbl.AutoSize = true;
        behaviorCaptionLbl.Font = new Font("Segoe UI Semibold", 9.5F);
        behaviorCaptionLbl.ForeColor = Color.FromArgb(28, 28, 30);
        behaviorCaptionLbl.Location = new Point(24, 828);
        behaviorCaptionLbl.Name = "behaviorCaptionLbl";
        behaviorCaptionLbl.Size = new Size(61, 17);
        behaviorCaptionLbl.TabIndex = 18;
        behaviorCaptionLbl.Text = "Behavior";
        // 
        // startAtLoginChk
        // 
        startAtLoginChk.AutoSize = true;
        startAtLoginChk.Font = new Font("Segoe UI", 9.5F);
        startAtLoginChk.ForeColor = Color.FromArgb(28, 28, 30);
        startAtLoginChk.Location = new Point(24, 852);
        startAtLoginChk.Name = "startAtLoginChk";
        startAtLoginChk.Size = new Size(401, 21);
        startAtLoginChk.TabIndex = 19;
        startAtLoginChk.Text = "Start TotalRecall when I sign in to Windows (launches minimized)";
        // 
        // minToTrayChk
        // 
        minToTrayChk.AutoSize = true;
        minToTrayChk.Font = new Font("Segoe UI", 9.5F);
        minToTrayChk.ForeColor = Color.FromArgb(28, 28, 30);
        minToTrayChk.Location = new Point(24, 878);
        minToTrayChk.Name = "minToTrayChk";
        minToTrayChk.Size = new Size(494, 21);
        minToTrayChk.TabIndex = 20;
        minToTrayChk.Text = "Minimize to system tray instead of taskbar (closing the window keeps it running)";
        // 
        // retentionCaptionLbl
        // 
        retentionCaptionLbl.AutoSize = true;
        retentionCaptionLbl.Font = new Font("Segoe UI Semibold", 9.5F);
        retentionCaptionLbl.ForeColor = Color.FromArgb(28, 28, 30);
        retentionCaptionLbl.Location = new Point(24, 918);
        retentionCaptionLbl.Name = "retentionCaptionLbl";
        retentionCaptionLbl.Size = new Size(113, 17);
        retentionCaptionLbl.TabIndex = 21;
        retentionCaptionLbl.Text = "History retention";
        // 
        // purgeImagesChk
        // 
        purgeImagesChk.AutoSize = true;
        purgeImagesChk.Font = new Font("Segoe UI", 9.5F);
        purgeImagesChk.ForeColor = Color.FromArgb(28, 28, 30);
        purgeImagesChk.Location = new Point(24, 942);
        purgeImagesChk.Name = "purgeImagesChk";
        purgeImagesChk.Size = new Size(201, 21);
        purgeImagesChk.TabIndex = 22;
        purgeImagesChk.Text = "Delete screenshots older than";
        // 
        // purgeImagesNud
        // 
        purgeImagesNud.Location = new Point(227, 941);
        purgeImagesNud.Maximum = new decimal(new int[] { 3650, 0, 0, 0 });
        purgeImagesNud.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        purgeImagesNud.Name = "purgeImagesNud";
        purgeImagesNud.Size = new Size(50, 23);
        purgeImagesNud.TabIndex = 23;
        purgeImagesNud.Value = new decimal(new int[] { 30, 0, 0, 0 });
        // 
        // purgeImagesDaysLbl
        // 
        purgeImagesDaysLbl.AutoSize = true;
        purgeImagesDaysLbl.Font = new Font("Segoe UI", 9.5F);
        purgeImagesDaysLbl.ForeColor = Color.FromArgb(102, 102, 108);
        purgeImagesDaysLbl.Location = new Point(282, 943);
        purgeImagesDaysLbl.Name = "purgeImagesDaysLbl";
        purgeImagesDaysLbl.Size = new Size(344, 17);
        purgeImagesDaysLbl.TabIndex = 24;
        purgeImagesDaysLbl.Text = "days  (keeps OCR text + metadata; saves the most space)";
        // 
        // purgeAllChk
        // 
        purgeAllChk.AutoSize = true;
        purgeAllChk.Font = new Font("Segoe UI", 9.5F);
        purgeAllChk.ForeColor = Color.FromArgb(28, 28, 30);
        purgeAllChk.Location = new Point(24, 970);
        purgeAllChk.Name = "purgeAllChk";
        purgeAllChk.Size = new Size(287, 21);
        purgeAllChk.TabIndex = 25;
        purgeAllChk.Text = "Delete screenshots AND OCR text older than";
        // 
        // purgeAllNud
        // 
        purgeAllNud.Location = new Point(314, 969);
        purgeAllNud.Maximum = new decimal(new int[] { 3650, 0, 0, 0 });
        purgeAllNud.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        purgeAllNud.Name = "purgeAllNud";
        purgeAllNud.Size = new Size(50, 23);
        purgeAllNud.TabIndex = 26;
        purgeAllNud.Value = new decimal(new int[] { 90, 0, 0, 0 });
        // 
        // purgeAllDaysLbl
        // 
        purgeAllDaysLbl.AutoSize = true;
        purgeAllDaysLbl.Font = new Font("Segoe UI", 9.5F);
        purgeAllDaysLbl.ForeColor = Color.FromArgb(102, 102, 108);
        purgeAllDaysLbl.Location = new Point(368, 971);
        purgeAllDaysLbl.Name = "purgeAllDaysLbl";
        purgeAllDaysLbl.Size = new Size(232, 17);
        purgeAllDaysLbl.TabIndex = 27;
        purgeAllDaysLbl.Text = "days  (full removal; cannot be undone)";
        // 
        // compactAfterRetentionChk
        // 
        compactAfterRetentionChk.AutoSize = true;
        compactAfterRetentionChk.Checked = true;
        compactAfterRetentionChk.CheckState = CheckState.Checked;
        compactAfterRetentionChk.Font = new Font("Segoe UI", 9.5F);
        compactAfterRetentionChk.ForeColor = Color.FromArgb(28, 28, 30);
        compactAfterRetentionChk.Location = new Point(24, 1000);
        compactAfterRetentionChk.Name = "compactAfterRetentionChk";
        compactAfterRetentionChk.Size = new Size(224, 21);
        compactAfterRetentionChk.TabIndex = 38;
        compactAfterRetentionChk.Text = "Compact database after retention";
        // 
        // compactAfterRetentionHoursNud
        // 
        compactAfterRetentionHoursNud.Location = new Point(253, 999);
        compactAfterRetentionHoursNud.Maximum = new decimal(new int[] { 720, 0, 0, 0 });
        compactAfterRetentionHoursNud.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        compactAfterRetentionHoursNud.Name = "compactAfterRetentionHoursNud";
        compactAfterRetentionHoursNud.Size = new Size(50, 23);
        compactAfterRetentionHoursNud.TabIndex = 39;
        compactAfterRetentionHoursNud.Value = new decimal(new int[] { 24, 0, 0, 0 });
        // 
        // compactAfterRetentionHoursLbl
        // 
        compactAfterRetentionHoursLbl.AutoSize = true;
        compactAfterRetentionHoursLbl.Font = new Font("Segoe UI", 9.5F);
        compactAfterRetentionHoursLbl.ForeColor = Color.FromArgb(102, 102, 108);
        compactAfterRetentionHoursLbl.Location = new Point(309, 1001);
        compactAfterRetentionHoursLbl.Name = "compactAfterRetentionHoursLbl";
        compactAfterRetentionHoursLbl.Size = new Size(271, 17);
        compactAfterRetentionHoursLbl.TabIndex = 40;
        compactAfterRetentionHoursLbl.Text = "hour minimum interval (Purge now overrides)";
        // 
        // purgeNowBtn
        // 
        purgeNowBtn.BackColor = Color.FromArgb(235, 235, 238);
        purgeNowBtn.Cursor = Cursors.Hand;
        purgeNowBtn.FlatAppearance.BorderColor = Color.FromArgb(210, 210, 215);
        purgeNowBtn.FlatStyle = FlatStyle.Flat;
        purgeNowBtn.Font = new Font("Segoe UI Semibold", 9.5F);
        purgeNowBtn.ForeColor = Color.FromArgb(28, 28, 30);
        purgeNowBtn.Location = new Point(24, 1042);
        purgeNowBtn.Name = "purgeNowBtn";
        purgeNowBtn.Padding = new Padding(8, 0, 8, 0);
        purgeNowBtn.Size = new Size(140, 32);
        purgeNowBtn.TabIndex = 28;
        purgeNowBtn.Text = "\U0001f9f9  Purge now";
        purgeNowBtn.UseVisualStyleBackColor = false;
        // 
        // clearDbBtn
        // 
        clearDbBtn.BackColor = Color.FromArgb(209, 48, 48);
        clearDbBtn.Cursor = Cursors.Hand;
        clearDbBtn.FlatAppearance.BorderColor = Color.FromArgb(180, 30, 30);
        clearDbBtn.FlatStyle = FlatStyle.Flat;
        clearDbBtn.Font = new Font("Segoe UI Semibold", 9.5F);
        clearDbBtn.ForeColor = Color.White;
        clearDbBtn.Location = new Point(180, 1042);
        clearDbBtn.Name = "clearDbBtn";
        clearDbBtn.Padding = new Padding(8, 0, 8, 0);
        clearDbBtn.Size = new Size(220, 32);
        clearDbBtn.TabIndex = 28;
        clearDbBtn.Text = "🗑  Clear database (delete all)";
        clearDbBtn.UseVisualStyleBackColor = false;
        // 
        // saveBtn
        // 
        saveBtn.BackColor = Color.FromArgb(76, 154, 255);
        saveBtn.Cursor = Cursors.Hand;
        saveBtn.FlatAppearance.BorderColor = Color.FromArgb(210, 210, 215);
        saveBtn.FlatStyle = FlatStyle.Flat;
        saveBtn.Font = new Font("Segoe UI Semibold", 9.5F);
        saveBtn.ForeColor = Color.White;
        saveBtn.Location = new Point(24, 1128);
        saveBtn.Name = "saveBtn";
        saveBtn.Padding = new Padding(8, 0, 8, 0);
        saveBtn.Size = new Size(180, 36);
        saveBtn.TabIndex = 29;
        saveBtn.Text = "💾  Save && Close";
        saveBtn.UseVisualStyleBackColor = false;
        // 
        // cancelBtn
        // 
        cancelBtn.BackColor = Color.White;
        cancelBtn.Cursor = Cursors.Hand;
        cancelBtn.FlatAppearance.BorderColor = Color.FromArgb(210, 210, 215);
        cancelBtn.FlatStyle = FlatStyle.Flat;
        cancelBtn.Font = new Font("Segoe UI Semibold", 9.5F);
        cancelBtn.ForeColor = Color.FromArgb(28, 28, 30);
        cancelBtn.Location = new Point(216, 1128);
        cancelBtn.Name = "cancelBtn";
        cancelBtn.Padding = new Padding(8, 0, 8, 0);
        cancelBtn.Size = new Size(120, 36);
        cancelBtn.TabIndex = 30;
        cancelBtn.Text = "Cancel";
        cancelBtn.UseVisualStyleBackColor = false;
        // 
        // SettingsPanel
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        AutoScroll = true;
        BackColor = Color.FromArgb(245, 245, 247);
        Controls.Add(card);
        Name = "SettingsPanel";
        Padding = new Padding(20);
        Size = new Size(783, 920);
        card.ResumeLayout(false);
        card.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)intervalNud).EndInit();
        ((System.ComponentModel.ISupportInitialize)qualityBar).EndInit();
        ((System.ComponentModel.ISupportInitialize)ocrMaxDimensionNud).EndInit();
        ((System.ComponentModel.ISupportInitialize)purgeImagesNud).EndInit();
        ((System.ComponentModel.ISupportInitialize)purgeAllNud).EndInit();
        ((System.ComponentModel.ISupportInitialize)compactAfterRetentionHoursNud).EndInit();
        ResumeLayout(false);
    }

    private System.Windows.Forms.Panel card;
    private System.Windows.Forms.Label titleLbl;
    private System.Windows.Forms.Label intervalCaptionLbl;
    private System.Windows.Forms.NumericUpDown intervalNud;
    private System.Windows.Forms.Label qualityCaptionLbl;
    private System.Windows.Forms.TrackBar qualityBar;
    private System.Windows.Forms.Label qualityValueLbl;
    private System.Windows.Forms.CheckBox storeImagesChk;
    private System.Windows.Forms.Label performanceCaptionLbl;
    private System.Windows.Forms.CheckBox changeDetectionChk;
    private System.Windows.Forms.CheckBox foregroundOnlyChk;
    private System.Windows.Forms.Label excludedAppsCaptionLbl;
    private System.Windows.Forms.TextBox excludedAppsTxt;
    private System.Windows.Forms.Label ocrMaxDimensionCaptionLbl;
    private System.Windows.Forms.NumericUpDown ocrMaxDimensionNud;
    private System.Windows.Forms.Label dbPathCaptionLbl;
    private System.Windows.Forms.TextBox dbPathTxt;
    private System.Windows.Forms.Button dbBrowseBtn;
    private System.Windows.Forms.Label langCaptionLbl;
    private System.Windows.Forms.ComboBox langCombo;
    private System.Windows.Forms.Label encCaptionLbl;
    private System.Windows.Forms.RadioButton encNoneRb;
    private System.Windows.Forms.RadioButton encUserRb;
    private System.Windows.Forms.RadioButton encPassRb;
    private System.Windows.Forms.TextBox passTxt;
    private System.Windows.Forms.Label hintLbl;
    private System.Windows.Forms.Label behaviorCaptionLbl;
    private System.Windows.Forms.CheckBox startAtLoginChk;
    private System.Windows.Forms.CheckBox minToTrayChk;
    private System.Windows.Forms.Label retentionCaptionLbl;
    private System.Windows.Forms.CheckBox purgeImagesChk;
    private System.Windows.Forms.NumericUpDown purgeImagesNud;
    private System.Windows.Forms.Label purgeImagesDaysLbl;
    private System.Windows.Forms.CheckBox purgeAllChk;
    private System.Windows.Forms.NumericUpDown purgeAllNud;
    private System.Windows.Forms.Label purgeAllDaysLbl;
    private System.Windows.Forms.CheckBox compactAfterRetentionChk;
    private System.Windows.Forms.NumericUpDown compactAfterRetentionHoursNud;
    private System.Windows.Forms.Label compactAfterRetentionHoursLbl;
    private System.Windows.Forms.Button purgeNowBtn;
    private System.Windows.Forms.Button clearDbBtn;
    private System.Windows.Forms.Button saveBtn;
    private System.Windows.Forms.Button cancelBtn;
    private Panel panel1;
}
