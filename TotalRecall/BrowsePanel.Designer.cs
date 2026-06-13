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

partial class BrowsePanel
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
        searchCard = new Panel();
        lblQuery = new Label();
        searchBox = new TextBox();
        lblApp = new Label();
        appCombo = new ComboBox();
        lblDate = new Label();
        useDateRange = new CheckBox();
        fromDate = new DateTimePicker();
        toDate = new DateTimePicker();
        lblLim = new Label();
        limitNud = new NumericUpDown();
        searchBtn = new Button();
        refreshBtn = new Button();
        resultsCountLbl = new Label();
        topSpacer = new Panel();
        split = new SplitContainer();
        results = new ListView();
        colWhen = new ColumnHeader();
        colApp = new ColumnHeader();
        colTitle = new ColumnHeader();
        colSnippet = new ColumnHeader();
        colImgKb = new ColumnHeader();
        colChars = new ColumnHeader();
        rightLayout = new TableLayoutPanel();
        previewTitleLbl = new Label();
        previewMetaLbl = new Label();
        preview = new PictureBox();
        previewText = new TextBox();
        searchCard.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)limitNud).BeginInit();
        ((System.ComponentModel.ISupportInitialize)split).BeginInit();
        split.Panel1.SuspendLayout();
        split.Panel2.SuspendLayout();
        split.SuspendLayout();
        rightLayout.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)preview).BeginInit();
        SuspendLayout();
        // 
        // searchCard
        // 
        searchCard.BackColor = Color.FromArgb(255, 255, 255);
        searchCard.Controls.Add(lblQuery);
        searchCard.Controls.Add(searchBox);
        searchCard.Controls.Add(lblApp);
        searchCard.Controls.Add(appCombo);
        searchCard.Controls.Add(lblDate);
        searchCard.Controls.Add(useDateRange);
        searchCard.Controls.Add(fromDate);
        searchCard.Controls.Add(toDate);
        searchCard.Controls.Add(lblLim);
        searchCard.Controls.Add(limitNud);
        searchCard.Controls.Add(searchBtn);
        searchCard.Controls.Add(refreshBtn);
        searchCard.Controls.Add(resultsCountLbl);
        searchCard.Dock = DockStyle.Top;
        searchCard.Location = new Point(20, 20);
        searchCard.Name = "searchCard";
        searchCard.Padding = new Padding(16, 12, 16, 12);
        searchCard.Size = new Size(1060, 110);
        searchCard.TabIndex = 2;
        // 
        // lblQuery
        // 
        lblQuery.AutoSize = true;
        lblQuery.Font = new Font("Segoe UI", 8.5F);
        lblQuery.ForeColor = Color.FromArgb(102, 102, 108);
        lblQuery.Location = new Point(16, 8);
        lblQuery.Name = "lblQuery";
        lblQuery.Size = new Size(42, 15);
        lblQuery.TabIndex = 0;
        lblQuery.Text = "Search";
        // 
        // searchBox
        // 
        searchBox.BackColor = Color.FromArgb(245, 245, 247);
        searchBox.BorderStyle = BorderStyle.FixedSingle;
        searchBox.Font = new Font("Segoe UI", 9.5F);
        searchBox.ForeColor = Color.FromArgb(28, 28, 30);
        searchBox.Location = new Point(16, 30);
        searchBox.Name = "searchBox";
        searchBox.PlaceholderText = "e.g. invoice OR \"q3 plan\"  →  title/app/text";
        searchBox.Size = new Size(360, 24);
        searchBox.TabIndex = 1;
        // 
        // lblApp
        // 
        lblApp.AutoSize = true;
        lblApp.Font = new Font("Segoe UI", 8.5F);
        lblApp.ForeColor = Color.FromArgb(102, 102, 108);
        lblApp.Location = new Point(390, 8);
        lblApp.Name = "lblApp";
        lblApp.Size = new Size(29, 15);
        lblApp.TabIndex = 2;
        lblApp.Text = "App";
        // 
        // appCombo
        // 
        appCombo.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
        appCombo.AutoCompleteSource = AutoCompleteSource.ListItems;
        appCombo.BackColor = Color.FromArgb(245, 245, 247);
        appCombo.FlatStyle = FlatStyle.Flat;
        appCombo.ForeColor = Color.FromArgb(28, 28, 30);
        appCombo.Location = new Point(390, 30);
        appCombo.Name = "appCombo";
        appCombo.Size = new Size(200, 23);
        appCombo.TabIndex = 3;
        // 
        // lblDate
        // 
        lblDate.AutoSize = true;
        lblDate.Font = new Font("Segoe UI", 8.5F);
        lblDate.ForeColor = Color.FromArgb(102, 102, 108);
        lblDate.Location = new Point(604, 8);
        lblDate.Name = "lblDate";
        lblDate.Size = new Size(64, 15);
        lblDate.TabIndex = 4;
        lblDate.Text = "Date range";
        // 
        // useDateRange
        // 
        useDateRange.AutoSize = true;
        useDateRange.Font = new Font("Segoe UI", 9.5F);
        useDateRange.ForeColor = Color.FromArgb(102, 102, 108);
        useDateRange.Location = new Point(604, 30);
        useDateRange.Name = "useDateRange";
        useDateRange.Size = new Size(53, 21);
        useDateRange.TabIndex = 5;
        useDateRange.Text = "filter";
        // 
        // fromDate
        // 
        fromDate.Format = DateTimePickerFormat.Short;
        fromDate.Location = new Point(660, 28);
        fromDate.Name = "fromDate";
        fromDate.Size = new Size(130, 23);
        fromDate.TabIndex = 6;
        // 
        // toDate
        // 
        toDate.Format = DateTimePickerFormat.Short;
        toDate.Location = new Point(800, 28);
        toDate.Name = "toDate";
        toDate.Size = new Size(130, 23);
        toDate.TabIndex = 7;
        // 
        // lblLim
        // 
        lblLim.AutoSize = true;
        lblLim.Font = new Font("Segoe UI", 8.5F);
        lblLim.ForeColor = Color.FromArgb(102, 102, 108);
        lblLim.Location = new Point(944, 8);
        lblLim.Name = "lblLim";
        lblLim.Size = new Size(34, 15);
        lblLim.TabIndex = 8;
        lblLim.Text = "Limit";
        // 
        // limitNud
        // 
        limitNud.Location = new Point(944, 30);
        limitNud.Maximum = new decimal(new int[] { 5000, 0, 0, 0 });
        limitNud.Minimum = new decimal(new int[] { 10, 0, 0, 0 });
        limitNud.Name = "limitNud";
        limitNud.Size = new Size(70, 23);
        limitNud.TabIndex = 9;
        limitNud.Value = new decimal(new int[] { 200, 0, 0, 0 });
        // 
        // searchBtn
        // 
        searchBtn.BackColor = Color.FromArgb(76, 154, 255);
        searchBtn.Cursor = Cursors.Hand;
        searchBtn.FlatAppearance.BorderColor = Color.FromArgb(210, 210, 215);
        searchBtn.FlatStyle = FlatStyle.Flat;
        searchBtn.Font = new Font("Segoe UI Semibold", 9.5F);
        searchBtn.ForeColor = Color.White;
        searchBtn.Location = new Point(16, 68);
        searchBtn.Name = "searchBtn";
        searchBtn.Padding = new Padding(8, 0, 8, 0);
        searchBtn.Size = new Size(120, 36);
        searchBtn.TabIndex = 10;
        searchBtn.Text = "🔍  Search";
        searchBtn.UseVisualStyleBackColor = false;
        searchBtn.Click += searchBtn_Click;
        // 
        // refreshBtn
        // 
        refreshBtn.BackColor = Color.FromArgb(235, 235, 238);
        refreshBtn.Cursor = Cursors.Hand;
        refreshBtn.FlatAppearance.BorderColor = Color.FromArgb(210, 210, 215);
        refreshBtn.FlatStyle = FlatStyle.Flat;
        refreshBtn.Font = new Font("Segoe UI Semibold", 9.5F);
        refreshBtn.ForeColor = Color.FromArgb(28, 28, 30);
        refreshBtn.Location = new Point(146, 68);
        refreshBtn.Name = "refreshBtn";
        refreshBtn.Padding = new Padding(8, 0, 8, 0);
        refreshBtn.Size = new Size(100, 36);
        refreshBtn.TabIndex = 11;
        refreshBtn.Text = "↻  Reset";
        refreshBtn.UseVisualStyleBackColor = false;
        // 
        // resultsCountLbl
        // 
        resultsCountLbl.AutoSize = true;
        resultsCountLbl.Font = new Font("Segoe UI", 9.5F);
        resultsCountLbl.ForeColor = Color.FromArgb(102, 102, 108);
        resultsCountLbl.Location = new Point(260, 76);
        resultsCountLbl.Name = "resultsCountLbl";
        resultsCountLbl.Size = new Size(21, 17);
        resultsCountLbl.TabIndex = 12;
        resultsCountLbl.Text = "—";
        // 
        // topSpacer
        // 
        topSpacer.BackColor = Color.FromArgb(245, 245, 247);
        topSpacer.Dock = DockStyle.Top;
        topSpacer.Location = new Point(20, 130);
        topSpacer.Name = "topSpacer";
        topSpacer.Size = new Size(1060, 12);
        topSpacer.TabIndex = 1;
        // 
        // split
        // 
        split.BackColor = Color.FromArgb(245, 245, 247);
        split.Cursor = Cursors.SizeWE;
        split.Dock = DockStyle.Fill;
        split.Location = new Point(20, 142);
        split.Name = "split";
        // 
        // split.Panel1
        // 
        split.Panel1.BackColor = Color.FromArgb(255, 255, 255);
        split.Panel1.Controls.Add(results);
        split.Panel1.Cursor = Cursors.Default;
        split.Panel1.Padding = new Padding(12);
        // 
        // split.Panel2
        // 
        split.Panel2.BackColor = Color.FromArgb(255, 255, 255);
        split.Panel2.Controls.Add(rightLayout);
        split.Panel2.Cursor = Cursors.Default;
        split.Panel2.Padding = new Padding(12);
        split.Size = new Size(1060, 458);
        split.SplitterDistance = 700;
        split.SplitterWidth = 6;
        split.TabIndex = 0;
        // 
        // results
        // 
        results.BackColor = Color.FromArgb(245, 245, 247);
        results.BorderStyle = BorderStyle.None;
        results.Columns.AddRange(new ColumnHeader[] { colWhen, colApp, colTitle, colSnippet, colImgKb, colChars });
        results.Dock = DockStyle.Fill;
        results.Font = new Font("Segoe UI", 9.5F);
        results.ForeColor = Color.FromArgb(28, 28, 30);
        results.FullRowSelect = true;
        results.HeaderStyle = ColumnHeaderStyle.Nonclickable;
        results.Location = new Point(12, 12);
        results.MultiSelect = false;
        results.Name = "results";
        results.Size = new Size(676, 434);
        results.TabIndex = 0;
        results.UseCompatibleStateImageBehavior = false;
        results.View = View.Details;
        // 
        // colWhen
        // 
        colWhen.Text = "When";
        colWhen.Width = 130;
        // 
        // colApp
        // 
        colApp.Text = "App";
        colApp.Width = 130;
        // 
        // colTitle
        // 
        colTitle.Text = "Title";
        colTitle.Width = 220;
        // 
        // colSnippet
        // 
        colSnippet.Text = "Snippet";
        colSnippet.Width = 280;
        // 
        // colImgKb
        // 
        colImgKb.Text = "Img KB";
        // 
        // colChars
        // 
        colChars.Text = "Chars";
        // 
        // rightLayout
        // 
        rightLayout.BackColor = Color.FromArgb(255, 255, 255);
        rightLayout.ColumnCount = 1;
        rightLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        rightLayout.Controls.Add(previewTitleLbl, 0, 0);
        rightLayout.Controls.Add(previewMetaLbl, 0, 1);
        rightLayout.Controls.Add(preview, 0, 2);
        rightLayout.Controls.Add(previewText, 0, 3);
        rightLayout.Dock = DockStyle.Fill;
        rightLayout.Location = new Point(12, 12);
        rightLayout.Name = "rightLayout";
        rightLayout.RowCount = 4;
        rightLayout.RowStyles.Add(new RowStyle());
        rightLayout.RowStyles.Add(new RowStyle());
        rightLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 60F));
        rightLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 40F));
        rightLayout.Size = new Size(330, 434);
        rightLayout.TabIndex = 0;
        // 
        // previewTitleLbl
        // 
        previewTitleLbl.AutoSize = true;
        previewTitleLbl.Dock = DockStyle.Fill;
        previewTitleLbl.Font = new Font("Segoe UI Semibold", 11F);
        previewTitleLbl.ForeColor = Color.FromArgb(28, 28, 30);
        previewTitleLbl.Location = new Point(3, 0);
        previewTitleLbl.Name = "previewTitleLbl";
        previewTitleLbl.Padding = new Padding(4, 0, 4, 4);
        previewTitleLbl.Size = new Size(324, 24);
        previewTitleLbl.TabIndex = 0;
        previewTitleLbl.Text = "Select a result to preview";
        // 
        // previewMetaLbl
        // 
        previewMetaLbl.AutoSize = true;
        previewMetaLbl.Dock = DockStyle.Fill;
        previewMetaLbl.Font = new Font("Segoe UI", 9.5F);
        previewMetaLbl.ForeColor = Color.FromArgb(102, 102, 108);
        previewMetaLbl.Location = new Point(3, 24);
        previewMetaLbl.Name = "previewMetaLbl";
        previewMetaLbl.Padding = new Padding(4, 0, 4, 8);
        previewMetaLbl.Size = new Size(324, 25);
        previewMetaLbl.TabIndex = 1;
        // 
        // preview
        // 
        preview.BackColor = Color.FromArgb(245, 245, 247);
        preview.BorderStyle = BorderStyle.FixedSingle;
        preview.Dock = DockStyle.Fill;
        preview.Location = new Point(3, 52);
        preview.Name = "preview";
        preview.Size = new Size(324, 225);
        preview.SizeMode = PictureBoxSizeMode.Zoom;
        preview.TabIndex = 2;
        preview.TabStop = false;
        // 
        // previewText
        // 
        previewText.BackColor = Color.FromArgb(245, 245, 247);
        previewText.BorderStyle = BorderStyle.FixedSingle;
        previewText.Dock = DockStyle.Fill;
        previewText.Font = new Font("Cascadia Mono", 9.5F);
        previewText.ForeColor = Color.FromArgb(28, 28, 30);
        previewText.Location = new Point(3, 283);
        previewText.Multiline = true;
        previewText.Name = "previewText";
        previewText.ReadOnly = true;
        previewText.ScrollBars = ScrollBars.Vertical;
        previewText.Size = new Size(324, 148);
        previewText.TabIndex = 3;
        // 
        // BrowsePanel
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(245, 245, 247);
        Controls.Add(split);
        Controls.Add(topSpacer);
        Controls.Add(searchCard);
        Name = "BrowsePanel";
        Padding = new Padding(20);
        Size = new Size(1100, 620);
        searchCard.ResumeLayout(false);
        searchCard.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)limitNud).EndInit();
        split.Panel1.ResumeLayout(false);
        split.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)split).EndInit();
        split.ResumeLayout(false);
        rightLayout.ResumeLayout(false);
        rightLayout.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)preview).EndInit();
        ResumeLayout(false);
    }

    private System.Windows.Forms.Panel searchCard;
    private System.Windows.Forms.Label lblQuery;
    private System.Windows.Forms.TextBox searchBox;
    private System.Windows.Forms.Label lblApp;
    private System.Windows.Forms.ComboBox appCombo;
    private System.Windows.Forms.Label lblDate;
    private System.Windows.Forms.CheckBox useDateRange;
    private System.Windows.Forms.DateTimePicker fromDate;
    private System.Windows.Forms.DateTimePicker toDate;
    private System.Windows.Forms.Label lblLim;
    private System.Windows.Forms.NumericUpDown limitNud;
    private System.Windows.Forms.Button searchBtn;
    private System.Windows.Forms.Button refreshBtn;
    private System.Windows.Forms.Label resultsCountLbl;
    private System.Windows.Forms.Panel topSpacer;
    private System.Windows.Forms.SplitContainer split;
    private System.Windows.Forms.ListView results;
    private System.Windows.Forms.ColumnHeader colWhen;
    private System.Windows.Forms.ColumnHeader colApp;
    private System.Windows.Forms.ColumnHeader colTitle;
    private System.Windows.Forms.ColumnHeader colSnippet;
    private System.Windows.Forms.ColumnHeader colImgKb;
    private System.Windows.Forms.ColumnHeader colChars;
    private System.Windows.Forms.TableLayoutPanel rightLayout;
    private System.Windows.Forms.Label previewTitleLbl;
    private System.Windows.Forms.Label previewMetaLbl;
    private System.Windows.Forms.PictureBox preview;
    private System.Windows.Forms.TextBox previewText;
}
