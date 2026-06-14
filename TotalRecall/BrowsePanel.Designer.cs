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
        components = new System.ComponentModel.Container();
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
        outerSplit = new SplitContainer();
        results = new ListView();
        colWhen = new ColumnHeader();
        colTitle = new ColumnHeader();
        colSnippet = new ColumnHeader();
        colApp = new ColumnHeader();
        innerSplit = new SplitContainer();
        previewContainer = new Panel();
        preview = new ZoomablePicturePanel();
        previewCtx = new ContextMenuStrip(components);
        previewToolbar = new Panel();
        previewTitleLbl = new Label();
        previewMetaLbl = new Label();
        zoomCaptionLbl = new Label();
        zoomBar = new TrackBar();
        zoomValueLbl = new Label();
        toggleSnippetBtn = new Button();
        textContainer = new Panel();
        previewText = new TextBox();
        textCtx = new ContextMenuStrip(components);
        panel1 = new Panel();
        textToolbar = new Panel();
        textTitleLbl = new Label();
        copyTextBtn = new Button();
        collapseTextBtn = new Button();
        searchCard.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)limitNud).BeginInit();
        ((System.ComponentModel.ISupportInitialize)outerSplit).BeginInit();
        outerSplit.Panel1.SuspendLayout();
        outerSplit.Panel2.SuspendLayout();
        outerSplit.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)innerSplit).BeginInit();
        innerSplit.Panel1.SuspendLayout();
        innerSplit.Panel2.SuspendLayout();
        innerSplit.SuspendLayout();
        previewContainer.SuspendLayout();
        previewToolbar.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)zoomBar).BeginInit();
        textContainer.SuspendLayout();
        textToolbar.SuspendLayout();
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
        searchCard.Size = new Size(1341, 110);
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
        topSpacer.Size = new Size(1341, 12);
        topSpacer.TabIndex = 1;
        // 
        // outerSplit
        // 
        outerSplit.BackColor = Color.FromArgb(245, 245, 247);
        outerSplit.Dock = DockStyle.Fill;
        outerSplit.FixedPanel = FixedPanel.Panel1;
        outerSplit.Location = new Point(20, 142);
        outerSplit.Name = "outerSplit";
        // 
        // outerSplit.Panel1
        // 
        outerSplit.Panel1.BackColor = Color.FromArgb(255, 255, 255);
        outerSplit.Panel1.Controls.Add(results);
        outerSplit.Panel1.Padding = new Padding(12);
        // 
        // outerSplit.Panel2
        // 
        outerSplit.Panel2.BackColor = Color.FromArgb(245, 245, 247);
        outerSplit.Panel2.Controls.Add(innerSplit);
        outerSplit.Size = new Size(1341, 750);
        outerSplit.SplitterDistance = 394;
        outerSplit.SplitterWidth = 6;
        outerSplit.TabIndex = 0;
        // 
        // results
        // 
        results.BackColor = Color.FromArgb(245, 245, 247);
        results.BorderStyle = BorderStyle.None;
        results.Columns.AddRange(new ColumnHeader[] { colWhen, colTitle, colSnippet, colApp });
        results.Dock = DockStyle.Fill;
        results.Font = new Font("Segoe UI", 9.5F);
        results.ForeColor = Color.FromArgb(28, 28, 30);
        results.FullRowSelect = true;
        results.HeaderStyle = ColumnHeaderStyle.Nonclickable;
        results.Location = new Point(12, 12);
        results.MultiSelect = false;
        results.Name = "results";
        results.Size = new Size(370, 726);
        results.TabIndex = 0;
        results.UseCompatibleStateImageBehavior = false;
        results.View = View.Details;
        // 
        // colWhen
        // 
        colWhen.Text = "When";
        colWhen.Width = 140;
        // 
        // colTitle
        // 
        colTitle.Text = "Title";
        colTitle.Width = 220;
        // 
        // colSnippet
        // 
        colSnippet.Text = "Snippet";
        colSnippet.Width = 320;
        // 
        // colApp
        // 
        colApp.Text = "App";
        colApp.Width = 150;
        // 
        // innerSplit
        // 
        innerSplit.BackColor = Color.FromArgb(245, 245, 247);
        innerSplit.Dock = DockStyle.Fill;
        innerSplit.FixedPanel = FixedPanel.Panel2;
        innerSplit.Location = new Point(0, 0);
        innerSplit.Name = "innerSplit";
        // 
        // innerSplit.Panel1
        // 
        innerSplit.Panel1.BackColor = Color.FromArgb(255, 255, 255);
        innerSplit.Panel1.Controls.Add(previewContainer);
        // 
        // innerSplit.Panel2
        // 
        innerSplit.Panel2.BackColor = Color.FromArgb(255, 255, 255);
        innerSplit.Panel2.Controls.Add(textContainer);
        innerSplit.Panel2MinSize = 60;
        innerSplit.Size = new Size(941, 750);
        innerSplit.SplitterDistance = 576;
        innerSplit.SplitterWidth = 6;
        innerSplit.TabIndex = 0;
        // 
        // previewContainer
        // 
        previewContainer.BackColor = Color.FromArgb(255, 255, 255);
        previewContainer.Controls.Add(preview);
        previewContainer.Controls.Add(previewToolbar);
        previewContainer.Dock = DockStyle.Fill;
        previewContainer.Location = new Point(0, 0);
        previewContainer.Name = "previewContainer";
        previewContainer.Padding = new Padding(12);
        previewContainer.Size = new Size(576, 750);
        previewContainer.TabIndex = 0;
        // 
        // preview
        // 
        preview.AutoScroll = true;
        preview.BackColor = Color.FromArgb(245, 245, 247);
        preview.BorderStyle = BorderStyle.FixedSingle;
        preview.ContextMenuStrip = previewCtx;
        preview.Dock = DockStyle.Fill;
        preview.Location = new Point(12, 88);
        preview.Name = "preview";
        preview.Size = new Size(552, 650);
        preview.TabIndex = 1;
        // 
        // previewCtx
        // 
        previewCtx.Name = "previewCtx";
        previewCtx.Size = new Size(61, 4);
        // 
        // previewToolbar
        // 
        previewToolbar.BackColor = Color.FromArgb(255, 255, 255);
        previewToolbar.Controls.Add(previewTitleLbl);
        previewToolbar.Controls.Add(previewMetaLbl);
        previewToolbar.Controls.Add(zoomCaptionLbl);
        previewToolbar.Controls.Add(zoomBar);
        previewToolbar.Controls.Add(zoomValueLbl);
        previewToolbar.Controls.Add(toggleSnippetBtn);
        previewToolbar.Dock = DockStyle.Top;
        previewToolbar.Location = new Point(12, 12);
        previewToolbar.Name = "previewToolbar";
        previewToolbar.Size = new Size(552, 76);
        previewToolbar.TabIndex = 0;
        // 
        // previewTitleLbl
        // 
        previewTitleLbl.AutoEllipsis = true;
        previewTitleLbl.Font = new Font("Segoe UI Semibold", 10.5F);
        previewTitleLbl.ForeColor = Color.FromArgb(28, 28, 30);
        previewTitleLbl.Location = new Point(0, 0);
        previewTitleLbl.Name = "previewTitleLbl";
        previewTitleLbl.Padding = new Padding(2, 0, 2, 0);
        previewTitleLbl.Size = new Size(256, 22);
        previewTitleLbl.TabIndex = 0;
        previewTitleLbl.Text = "Select a result to preview";
        // 
        // previewMetaLbl
        // 
        previewMetaLbl.AutoEllipsis = true;
        previewMetaLbl.Font = new Font("Segoe UI", 9F);
        previewMetaLbl.ForeColor = Color.FromArgb(102, 102, 108);
        previewMetaLbl.Location = new Point(0, 22);
        previewMetaLbl.Name = "previewMetaLbl";
        previewMetaLbl.Padding = new Padding(2, 0, 2, 0);
        previewMetaLbl.Size = new Size(256, 20);
        previewMetaLbl.TabIndex = 1;
        // 
        // zoomCaptionLbl
        // 
        zoomCaptionLbl.AutoSize = true;
        zoomCaptionLbl.Font = new Font("Segoe UI", 8.5F);
        zoomCaptionLbl.ForeColor = Color.FromArgb(102, 102, 108);
        zoomCaptionLbl.Location = new Point(2, 50);
        zoomCaptionLbl.Name = "zoomCaptionLbl";
        zoomCaptionLbl.Size = new Size(39, 15);
        zoomCaptionLbl.TabIndex = 2;
        zoomCaptionLbl.Text = "Zoom";
        // 
        // zoomBar
        // 
        zoomBar.AutoSize = false;
        zoomBar.BackColor = Color.FromArgb(255, 255, 255);
        zoomBar.LargeChange = 1;
        zoomBar.Location = new Point(40, 48);
        zoomBar.Maximum = 6;
        zoomBar.Name = "zoomBar";
        zoomBar.Size = new Size(160, 28);
        zoomBar.TabIndex = 3;
        zoomBar.TickStyle = TickStyle.None;
        // 
        // zoomValueLbl
        // 
        zoomValueLbl.Font = new Font("Segoe UI", 9F);
        zoomValueLbl.ForeColor = Color.FromArgb(102, 102, 108);
        zoomValueLbl.Location = new Point(202, 50);
        zoomValueLbl.Name = "zoomValueLbl";
        zoomValueLbl.Size = new Size(40, 18);
        zoomValueLbl.TabIndex = 4;
        zoomValueLbl.Text = "Fit";
        // 
        // toggleSnippetBtn
        // 
        toggleSnippetBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        toggleSnippetBtn.BackColor = Color.FromArgb(235, 235, 238);
        toggleSnippetBtn.Cursor = Cursors.Hand;
        toggleSnippetBtn.FlatAppearance.BorderColor = Color.FromArgb(210, 210, 215);
        toggleSnippetBtn.FlatStyle = FlatStyle.Flat;
        toggleSnippetBtn.Font = new Font("Segoe UI Semibold", 9F);
        toggleSnippetBtn.ForeColor = Color.FromArgb(28, 28, 30);
        toggleSnippetBtn.Location = new Point(476, 0);
        toggleSnippetBtn.Name = "toggleSnippetBtn";
        toggleSnippetBtn.Padding = new Padding(6, 0, 6, 0);
        toggleSnippetBtn.Size = new Size(76, 25);
        toggleSnippetBtn.TabIndex = 5;
        toggleSnippetBtn.Text = "Snippet ▢";
        toggleSnippetBtn.UseVisualStyleBackColor = false;
        toggleSnippetBtn.Visible = false;
        // 
        // textContainer
        // 
        textContainer.BackColor = Color.FromArgb(255, 255, 255);
        textContainer.Controls.Add(previewText);
        textContainer.Controls.Add(panel1);
        textContainer.Controls.Add(textToolbar);
        textContainer.Dock = DockStyle.Fill;
        textContainer.Location = new Point(0, 0);
        textContainer.Name = "textContainer";
        textContainer.Padding = new Padding(12);
        textContainer.Size = new Size(359, 750);
        textContainer.TabIndex = 0;
        // 
        // previewText
        // 
        previewText.BackColor = Color.FromArgb(245, 245, 247);
        previewText.BorderStyle = BorderStyle.FixedSingle;
        previewText.ContextMenuStrip = textCtx;
        previewText.Dock = DockStyle.Fill;
        previewText.Font = new Font("Cascadia Mono", 9.5F);
        previewText.ForeColor = Color.FromArgb(28, 28, 30);
        previewText.Location = new Point(12, 54);
        previewText.Multiline = true;
        previewText.Name = "previewText";
        previewText.ReadOnly = true;
        previewText.ScrollBars = ScrollBars.Vertical;
        previewText.Size = new Size(335, 684);
        previewText.TabIndex = 1;
        // 
        // textCtx
        // 
        textCtx.Name = "textCtx";
        textCtx.Size = new Size(61, 4);
        // 
        // panel1
        // 
        panel1.Dock = DockStyle.Top;
        panel1.Location = new Point(12, 40);
        panel1.Name = "panel1";
        panel1.Size = new Size(335, 14);
        panel1.TabIndex = 2;
        // 
        // textToolbar
        // 
        textToolbar.BackColor = Color.FromArgb(255, 255, 255);
        textToolbar.Controls.Add(textTitleLbl);
        textToolbar.Controls.Add(copyTextBtn);
        textToolbar.Controls.Add(collapseTextBtn);
        textToolbar.Dock = DockStyle.Top;
        textToolbar.Location = new Point(12, 12);
        textToolbar.Name = "textToolbar";
        textToolbar.Size = new Size(335, 28);
        textToolbar.TabIndex = 0;
        // 
        // textTitleLbl
        // 
        textTitleLbl.Dock = DockStyle.Fill;
        textTitleLbl.Font = new Font("Segoe UI Semibold", 10F);
        textTitleLbl.ForeColor = Color.FromArgb(28, 28, 30);
        textTitleLbl.Location = new Point(0, 0);
        textTitleLbl.Name = "textTitleLbl";
        textTitleLbl.Padding = new Padding(2, 4, 2, 0);
        textTitleLbl.Size = new Size(235, 28);
        textTitleLbl.TabIndex = 0;
        textTitleLbl.Text = "Captured text";
        // 
        // copyTextBtn
        // 
        copyTextBtn.BackColor = Color.FromArgb(235, 235, 238);
        copyTextBtn.Cursor = Cursors.Hand;
        copyTextBtn.Dock = DockStyle.Right;
        copyTextBtn.FlatAppearance.BorderColor = Color.FromArgb(210, 210, 215);
        copyTextBtn.FlatStyle = FlatStyle.Flat;
        copyTextBtn.Font = new Font("Segoe UI Semibold", 9F);
        copyTextBtn.ForeColor = Color.FromArgb(28, 28, 30);
        copyTextBtn.Location = new Point(235, 0);
        copyTextBtn.Name = "copyTextBtn";
        copyTextBtn.Padding = new Padding(6, 0, 6, 0);
        copyTextBtn.Size = new Size(64, 28);
        copyTextBtn.TabIndex = 1;
        copyTextBtn.Text = "Copy";
        copyTextBtn.UseVisualStyleBackColor = false;
        // 
        // collapseTextBtn
        // 
        collapseTextBtn.BackColor = Color.FromArgb(235, 235, 238);
        collapseTextBtn.Cursor = Cursors.Hand;
        collapseTextBtn.Dock = DockStyle.Right;
        collapseTextBtn.FlatAppearance.BorderColor = Color.FromArgb(210, 210, 215);
        collapseTextBtn.FlatStyle = FlatStyle.Flat;
        collapseTextBtn.Font = new Font("Segoe UI Semibold", 9F);
        collapseTextBtn.ForeColor = Color.FromArgb(28, 28, 30);
        collapseTextBtn.Location = new Point(299, 0);
        collapseTextBtn.Name = "collapseTextBtn";
        collapseTextBtn.Padding = new Padding(6, 0, 6, 0);
        collapseTextBtn.Size = new Size(36, 28);
        collapseTextBtn.TabIndex = 2;
        collapseTextBtn.Text = "✕";
        collapseTextBtn.UseVisualStyleBackColor = false;
        // 
        // BrowsePanel
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(245, 245, 247);
        Controls.Add(outerSplit);
        Controls.Add(topSpacer);
        Controls.Add(searchCard);
        Name = "BrowsePanel";
        Padding = new Padding(20);
        Size = new Size(1381, 912);
        searchCard.ResumeLayout(false);
        searchCard.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)limitNud).EndInit();
        outerSplit.Panel1.ResumeLayout(false);
        outerSplit.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)outerSplit).EndInit();
        outerSplit.ResumeLayout(false);
        innerSplit.Panel1.ResumeLayout(false);
        innerSplit.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)innerSplit).EndInit();
        innerSplit.ResumeLayout(false);
        previewContainer.ResumeLayout(false);
        previewToolbar.ResumeLayout(false);
        previewToolbar.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)zoomBar).EndInit();
        textContainer.ResumeLayout(false);
        textContainer.PerformLayout();
        textToolbar.ResumeLayout(false);
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

    private System.Windows.Forms.SplitContainer outerSplit;
    private System.Windows.Forms.SplitContainer innerSplit;

    private System.Windows.Forms.ListView results;
    private System.Windows.Forms.ColumnHeader colWhen;
    private System.Windows.Forms.ColumnHeader colTitle;
    private System.Windows.Forms.ColumnHeader colSnippet;
    private System.Windows.Forms.ColumnHeader colApp;

    private System.Windows.Forms.Panel previewContainer;
    private System.Windows.Forms.Panel previewToolbar;
    private System.Windows.Forms.Label previewTitleLbl;
    private System.Windows.Forms.Label zoomCaptionLbl;
    private System.Windows.Forms.TrackBar zoomBar;
    private System.Windows.Forms.Label zoomValueLbl;
    private System.Windows.Forms.Button toggleSnippetBtn;
    private TotalRecall.ZoomablePicturePanel preview;
    private System.Windows.Forms.ContextMenuStrip previewCtx;

    private System.Windows.Forms.Panel textContainer;
    private System.Windows.Forms.Panel textToolbar;
    private System.Windows.Forms.Label textTitleLbl;
    private System.Windows.Forms.Button copyTextBtn;
    private System.Windows.Forms.Button collapseTextBtn;
    private System.Windows.Forms.TextBox previewText;
    private System.Windows.Forms.ContextMenuStrip textCtx;
    private Label previewMetaLbl;
    private Panel panel1;
}
