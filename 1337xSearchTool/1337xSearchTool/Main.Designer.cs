namespace _1337xSearchTool
{
    partial class Main
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.scContainer = new System.Windows.Forms.SplitContainer();
            this.gbHeader = new System.Windows.Forms.GroupBox();
            this.cbCategory = new System.Windows.Forms.ComboBox();
            this.chFilter = new System.Windows.Forms.CheckBox();
            this.tbFilter = new System.Windows.Forms.TextBox();
            this.cbOperators = new System.Windows.Forms.ComboBox();
            this.cbColumns = new System.Windows.Forms.ComboBox();
            this.btGoto = new System.Windows.Forms.Button();
            this.btSave = new System.Windows.Forms.Button();
            this.btLoad = new System.Windows.Forms.Button();
            this.btDownload = new System.Windows.Forms.Button();
            this.tlInfo = new System.Windows.Forms.TableLayoutPanel();
            this.lbPageItems = new System.Windows.Forms.Label();
            this.lbPageItemsValue = new System.Windows.Forms.Label();
            this.tbItemsToGetValue = new System.Windows.Forms.TextBox();
            this.lbItemsShown = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.lbItemsShownValue = new System.Windows.Forms.Label();
            this.lbLastUpdate = new System.Windows.Forms.Label();
            this.lbLastUpdateValue = new System.Windows.Forms.Label();
            this.lbInPage = new System.Windows.Forms.Label();
            this.lbInPageValue = new System.Windows.Forms.Label();
            this.btGet = new System.Windows.Forms.Button();
            this.tsContainer = new System.Windows.Forms.ToolStripContainer();
            this.ssStatusBar = new System.Windows.Forms.StatusStrip();
            this.tsStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsPBar = new System.Windows.Forms.ToolStripProgressBar();
            this.dgGrid = new _1337xSearchTool.CustomDataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.scContainer)).BeginInit();
            this.scContainer.Panel1.SuspendLayout();
            this.scContainer.Panel2.SuspendLayout();
            this.scContainer.SuspendLayout();
            this.gbHeader.SuspendLayout();
            this.tlInfo.SuspendLayout();
            this.tsContainer.BottomToolStripPanel.SuspendLayout();
            this.tsContainer.ContentPanel.SuspendLayout();
            this.tsContainer.SuspendLayout();
            this.ssStatusBar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // scContainer
            // 
            this.scContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.scContainer.IsSplitterFixed = true;
            this.scContainer.Location = new System.Drawing.Point(0, 0);
            this.scContainer.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.scContainer.Name = "scContainer";
            this.scContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // scContainer.Panel1
            // 
            this.scContainer.Panel1.Controls.Add(this.gbHeader);
            this.scContainer.Panel1.Padding = new System.Windows.Forms.Padding(13, 12, 13, 6);
            // 
            // scContainer.Panel2
            // 
            this.scContainer.Panel2.Controls.Add(this.tsContainer);
            this.scContainer.Size = new System.Drawing.Size(1084, 692);
            this.scContainer.SplitterDistance = 144;
            this.scContainer.SplitterWidth = 5;
            this.scContainer.TabIndex = 0;
            // 
            // gbHeader
            // 
            this.gbHeader.Controls.Add(this.cbCategory);
            this.gbHeader.Controls.Add(this.chFilter);
            this.gbHeader.Controls.Add(this.tbFilter);
            this.gbHeader.Controls.Add(this.cbOperators);
            this.gbHeader.Controls.Add(this.cbColumns);
            this.gbHeader.Controls.Add(this.btGoto);
            this.gbHeader.Controls.Add(this.btSave);
            this.gbHeader.Controls.Add(this.btLoad);
            this.gbHeader.Controls.Add(this.btDownload);
            this.gbHeader.Controls.Add(this.tlInfo);
            this.gbHeader.Controls.Add(this.btGet);
            this.gbHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbHeader.Location = new System.Drawing.Point(13, 12);
            this.gbHeader.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.gbHeader.Name = "gbHeader";
            this.gbHeader.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.gbHeader.Size = new System.Drawing.Size(1058, 126);
            this.gbHeader.TabIndex = 0;
            this.gbHeader.TabStop = false;
            // 
            // cbCategory
            // 
            this.cbCategory.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbCategory.FormattingEnabled = true;
            this.cbCategory.Location = new System.Drawing.Point(9, 94);
            this.cbCategory.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.cbCategory.Name = "cbCategory";
            this.cbCategory.Size = new System.Drawing.Size(124, 24);
            this.cbCategory.TabIndex = 6;
            this.cbCategory.SelectedIndexChanged += new System.EventHandler(this.cbCategory_SelectedIndexChanged);
            // 
            // chFilter
            // 
            this.chFilter.Appearance = System.Windows.Forms.Appearance.Button;
            this.chFilter.Location = new System.Drawing.Point(9, 64);
            this.chFilter.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.chFilter.Name = "chFilter";
            this.chFilter.Size = new System.Drawing.Size(100, 28);
            this.chFilter.TabIndex = 5;
            this.chFilter.Text = "Filter";
            this.chFilter.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.chFilter.UseVisualStyleBackColor = true;
            this.chFilter.CheckedChanged += new System.EventHandler(this.chFilter_CheckedChanged);
            // 
            // tbFilter
            // 
            this.tbFilter.HideSelection = false;
            this.tbFilter.Location = new System.Drawing.Point(268, 25);
            this.tbFilter.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tbFilter.Name = "tbFilter";
            this.tbFilter.Size = new System.Drawing.Size(313, 22);
            this.tbFilter.TabIndex = 4;
            this.tbFilter.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tbFilter_KeyDown);
            // 
            // cbOperators
            // 
            this.cbOperators.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbOperators.FormattingEnabled = true;
            this.cbOperators.Location = new System.Drawing.Point(143, 25);
            this.cbOperators.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.cbOperators.Name = "cbOperators";
            this.cbOperators.Size = new System.Drawing.Size(116, 24);
            this.cbOperators.TabIndex = 3;
            // 
            // cbColumns
            // 
            this.cbColumns.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbColumns.FormattingEnabled = true;
            this.cbColumns.Location = new System.Drawing.Point(9, 25);
            this.cbColumns.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.cbColumns.Name = "cbColumns";
            this.cbColumns.Size = new System.Drawing.Size(124, 24);
            this.cbColumns.TabIndex = 3;
            this.cbColumns.SelectedIndexChanged += new System.EventHandler(this.cbColumns_SelectedIndexChanged);
            // 
            // btGoto
            // 
            this.btGoto.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btGoto.Location = new System.Drawing.Point(264, 90);
            this.btGoto.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btGoto.Name = "btGoto";
            this.btGoto.Size = new System.Drawing.Size(100, 28);
            this.btGoto.TabIndex = 2;
            this.btGoto.Text = "Go to";
            this.btGoto.UseVisualStyleBackColor = true;
            this.btGoto.Click += new System.EventHandler(this.btGoto_Click);
            // 
            // btSave
            // 
            this.btSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btSave.Location = new System.Drawing.Point(644, 90);
            this.btSave.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btSave.Name = "btSave";
            this.btSave.Size = new System.Drawing.Size(100, 28);
            this.btSave.TabIndex = 2;
            this.btSave.Text = "Save";
            this.btSave.UseVisualStyleBackColor = true;
            this.btSave.Click += new System.EventHandler(this.btSave_Click);
            // 
            // btLoad
            // 
            this.btLoad.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btLoad.Location = new System.Drawing.Point(536, 90);
            this.btLoad.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btLoad.Name = "btLoad";
            this.btLoad.Size = new System.Drawing.Size(100, 28);
            this.btLoad.TabIndex = 2;
            this.btLoad.Text = "Load";
            this.btLoad.UseVisualStyleBackColor = true;
            this.btLoad.Click += new System.EventHandler(this.btLoad_Click);
            // 
            // btDownload
            // 
            this.btDownload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btDownload.Location = new System.Drawing.Point(372, 90);
            this.btDownload.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btDownload.Name = "btDownload";
            this.btDownload.Size = new System.Drawing.Size(100, 28);
            this.btDownload.TabIndex = 2;
            this.btDownload.Text = "Magnet";
            this.btDownload.UseVisualStyleBackColor = true;
            this.btDownload.Click += new System.EventHandler(this.btDownload_Click);
            // 
            // tlInfo
            // 
            this.tlInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.tlInfo.ColumnCount = 2;
            this.tlInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40.90909F));
            this.tlInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 59.09091F));
            this.tlInfo.Controls.Add(this.lbPageItems, 0, 0);
            this.tlInfo.Controls.Add(this.lbPageItemsValue, 1, 0);
            this.tlInfo.Controls.Add(this.tbItemsToGetValue, 1, 1);
            this.tlInfo.Controls.Add(this.lbItemsShown, 0, 2);
            this.tlInfo.Controls.Add(this.label1, 0, 1);
            this.tlInfo.Controls.Add(this.lbItemsShownValue, 1, 2);
            this.tlInfo.Controls.Add(this.lbLastUpdate, 0, 4);
            this.tlInfo.Controls.Add(this.lbLastUpdateValue, 1, 4);
            this.tlInfo.Controls.Add(this.lbInPage, 0, 3);
            this.tlInfo.Controls.Add(this.lbInPageValue, 1, 3);
            this.tlInfo.Location = new System.Drawing.Point(758, 14);
            this.tlInfo.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tlInfo.Name = "tlInfo";
            this.tlInfo.RowCount = 5;
            this.tlInfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 27F));
            this.tlInfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 27F));
            this.tlInfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 27F));
            this.tlInfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 27F));
            this.tlInfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 27F));
            this.tlInfo.Size = new System.Drawing.Size(293, 138);
            this.tlInfo.TabIndex = 1;
            // 
            // lbPageItems
            // 
            this.lbPageItems.AutoSize = true;
            this.lbPageItems.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbPageItems.Location = new System.Drawing.Point(4, 0);
            this.lbPageItems.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbPageItems.Name = "lbPageItems";
            this.lbPageItems.Size = new System.Drawing.Size(111, 27);
            this.lbPageItems.TabIndex = 0;
            this.lbPageItems.Text = "Items per page:";
            this.lbPageItems.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lbPageItemsValue
            // 
            this.lbPageItemsValue.AutoSize = true;
            this.lbPageItemsValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbPageItemsValue.Location = new System.Drawing.Point(123, 0);
            this.lbPageItemsValue.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbPageItemsValue.Name = "lbPageItemsValue";
            this.lbPageItemsValue.Size = new System.Drawing.Size(166, 27);
            this.lbPageItemsValue.TabIndex = 0;
            this.lbPageItemsValue.Text = "0";
            this.lbPageItemsValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tbItemsToGetValue
            // 
            this.tbItemsToGetValue.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.tbItemsToGetValue.Location = new System.Drawing.Point(123, 29);
            this.tbItemsToGetValue.Margin = new System.Windows.Forms.Padding(4, 2, 0, 0);
            this.tbItemsToGetValue.MaxLength = 4;
            this.tbItemsToGetValue.Name = "tbItemsToGetValue";
            this.tbItemsToGetValue.Size = new System.Drawing.Size(75, 22);
            this.tbItemsToGetValue.TabIndex = 1;
            this.tbItemsToGetValue.Text = "0";
            this.tbItemsToGetValue.TextChanged += new System.EventHandler(this.tbItemsToGetValue_TextChanged);
            // 
            // lbItemsShown
            // 
            this.lbItemsShown.AutoSize = true;
            this.lbItemsShown.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbItemsShown.Location = new System.Drawing.Point(4, 54);
            this.lbItemsShown.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbItemsShown.Name = "lbItemsShown";
            this.lbItemsShown.Size = new System.Drawing.Size(111, 27);
            this.lbItemsShown.TabIndex = 0;
            this.lbItemsShown.Text = "Items shown:";
            this.lbItemsShown.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(4, 27);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(111, 27);
            this.label1.TabIndex = 0;
            this.label1.Text = "Items to get:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lbItemsShownValue
            // 
            this.lbItemsShownValue.AutoSize = true;
            this.lbItemsShownValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbItemsShownValue.Location = new System.Drawing.Point(123, 54);
            this.lbItemsShownValue.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbItemsShownValue.Name = "lbItemsShownValue";
            this.lbItemsShownValue.Size = new System.Drawing.Size(166, 27);
            this.lbItemsShownValue.TabIndex = 0;
            this.lbItemsShownValue.Text = "0";
            this.lbItemsShownValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lbLastUpdate
            // 
            this.lbLastUpdate.AutoSize = true;
            this.lbLastUpdate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbLastUpdate.Location = new System.Drawing.Point(4, 108);
            this.lbLastUpdate.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbLastUpdate.Name = "lbLastUpdate";
            this.lbLastUpdate.Size = new System.Drawing.Size(111, 30);
            this.lbLastUpdate.TabIndex = 6;
            this.lbLastUpdate.Text = "Last update:";
            this.lbLastUpdate.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lbLastUpdateValue
            // 
            this.lbLastUpdateValue.AutoSize = true;
            this.lbLastUpdateValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbLastUpdateValue.Location = new System.Drawing.Point(123, 108);
            this.lbLastUpdateValue.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbLastUpdateValue.Name = "lbLastUpdateValue";
            this.lbLastUpdateValue.Size = new System.Drawing.Size(166, 30);
            this.lbLastUpdateValue.TabIndex = 6;
            this.lbLastUpdateValue.Text = "...";
            this.lbLastUpdateValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lbInPage
            // 
            this.lbInPage.AutoSize = true;
            this.lbInPage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbInPage.Location = new System.Drawing.Point(4, 81);
            this.lbInPage.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbInPage.Name = "lbInPage";
            this.lbInPage.Size = new System.Drawing.Size(111, 27);
            this.lbInPage.TabIndex = 0;
            this.lbInPage.Text = "Torrent in page:";
            this.lbInPage.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lbInPageValue
            // 
            this.lbInPageValue.AutoSize = true;
            this.lbInPageValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbInPageValue.Location = new System.Drawing.Point(123, 81);
            this.lbInPageValue.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbInPageValue.Name = "lbInPageValue";
            this.lbInPageValue.Size = new System.Drawing.Size(166, 27);
            this.lbInPageValue.TabIndex = 0;
            this.lbInPageValue.Text = "...";
            this.lbInPageValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btGet
            // 
            this.btGet.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btGet.Location = new System.Drawing.Point(156, 90);
            this.btGet.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btGet.Name = "btGet";
            this.btGet.Size = new System.Drawing.Size(100, 28);
            this.btGet.TabIndex = 0;
            this.btGet.Text = "Get Torrents";
            this.btGet.UseVisualStyleBackColor = true;
            this.btGet.Click += new System.EventHandler(this.btGet_Click);
            // 
            // tsContainer
            // 
            // 
            // tsContainer.BottomToolStripPanel
            // 
            this.tsContainer.BottomToolStripPanel.Controls.Add(this.ssStatusBar);
            // 
            // tsContainer.ContentPanel
            // 
            this.tsContainer.ContentPanel.Controls.Add(this.dgGrid);
            this.tsContainer.ContentPanel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tsContainer.ContentPanel.Padding = new System.Windows.Forms.Padding(13, 6, 13, 12);
            this.tsContainer.ContentPanel.Size = new System.Drawing.Size(1084, 472);
            this.tsContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tsContainer.Location = new System.Drawing.Point(0, 0);
            this.tsContainer.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tsContainer.Name = "tsContainer";
            this.tsContainer.Size = new System.Drawing.Size(1084, 543);
            this.tsContainer.TabIndex = 0;
            this.tsContainer.Text = "toolStripContainer1";
            // 
            // ssStatusBar
            // 
            this.ssStatusBar.AutoSize = false;
            this.ssStatusBar.Dock = System.Windows.Forms.DockStyle.None;
            this.ssStatusBar.ImageScalingSize = new System.Drawing.Size(22, 22);
            this.ssStatusBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsStatus,
            this.tsPBar});
            this.ssStatusBar.Location = new System.Drawing.Point(0, 0);
            this.ssStatusBar.Name = "ssStatusBar";
            this.ssStatusBar.Size = new System.Drawing.Size(1084, 40);
            this.ssStatusBar.SizingGrip = false;
            this.ssStatusBar.TabIndex = 0;
            // 
            // tsStatus
            // 
            this.tsStatus.AutoSize = false;
            this.tsStatus.Margin = new System.Windows.Forms.Padding(6, 3, 5, 2);
            this.tsStatus.Name = "tsStatus";
            this.tsStatus.Size = new System.Drawing.Size(250, 35);
            this.tsStatus.Text = "Status...";
            this.tsStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tsPBar
            // 
            this.tsPBar.AutoSize = false;
            this.tsPBar.Margin = new System.Windows.Forms.Padding(0, 9, 0, 9);
            this.tsPBar.MarqueeAnimationSpeed = 10;
            this.tsPBar.Name = "tsPBar";
            this.tsPBar.Size = new System.Drawing.Size(510, 22);
            this.tsPBar.Step = 1;
            this.tsPBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.tsPBar.Value = 50;
            // 
            // dgGrid
            // 
            this.dgGrid.BackgroundColor = System.Drawing.Color.White;
            this.dgGrid.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            this.dgGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgGrid.Location = new System.Drawing.Point(13, 6);
            this.dgGrid.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.dgGrid.Name = "dgGrid";
            this.dgGrid.RowHeadersWidth = 51;
            this.dgGrid.Size = new System.Drawing.Size(1058, 454);
            this.dgGrid.TabIndex = 0;
            this.dgGrid.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgGrid_CellClick);
            this.dgGrid.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgGrid_CellDoubleClick);
            this.dgGrid.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.dgGrid_CellFormatting);
            this.dgGrid.ColumnHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dgGrid_ColumnHeaderMouseClick);
            this.dgGrid.KeyDown += new System.Windows.Forms.KeyEventHandler(this.dgGrid_KeyDown);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CausesValidation = false;
            this.ClientSize = new System.Drawing.Size(1084, 692);
            this.Controls.Add(this.scContainer);
            this.DoubleBuffered = true;
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MinimumSize = new System.Drawing.Size(1099, 726);
            this.Name = "Main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "1337x.org - Search Tool";
            this.Load += new System.EventHandler(this.Main_Load);
            this.Shown += new System.EventHandler(this.Main_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Main_KeyDown);
            this.Resize += new System.EventHandler(this.Main_Resize);
            this.scContainer.Panel1.ResumeLayout(false);
            this.scContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.scContainer)).EndInit();
            this.scContainer.ResumeLayout(false);
            this.gbHeader.ResumeLayout(false);
            this.gbHeader.PerformLayout();
            this.tlInfo.ResumeLayout(false);
            this.tlInfo.PerformLayout();
            this.tsContainer.BottomToolStripPanel.ResumeLayout(false);
            this.tsContainer.ContentPanel.ResumeLayout(false);
            this.tsContainer.ResumeLayout(false);
            this.tsContainer.PerformLayout();
            this.ssStatusBar.ResumeLayout(false);
            this.ssStatusBar.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgGrid)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer scContainer;
        private System.Windows.Forms.GroupBox gbHeader;
        private System.Windows.Forms.ToolStripContainer tsContainer;
        private System.Windows.Forms.StatusStrip ssStatusBar;
        private CustomDataGridView dgGrid;
        private System.Windows.Forms.ToolStripStatusLabel tsStatus;
        private System.Windows.Forms.ToolStripProgressBar tsPBar;
        private System.Windows.Forms.TableLayoutPanel tlInfo;
        private System.Windows.Forms.Label lbPageItems;
        private System.Windows.Forms.Label lbPageItemsValue;
        private System.Windows.Forms.Label lbItemsShown;
        private System.Windows.Forms.Button btDownload;
        private System.Windows.Forms.TextBox tbItemsToGetValue;
        private System.Windows.Forms.Button btGet;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lbItemsShownValue;
        private System.Windows.Forms.Button btGoto;
        private System.Windows.Forms.TextBox tbFilter;
        private System.Windows.Forms.ComboBox cbOperators;
        private System.Windows.Forms.ComboBox cbColumns;
        private System.Windows.Forms.CheckBox chFilter;
        private System.Windows.Forms.Button btSave;
        private System.Windows.Forms.Button btLoad;
        private System.Windows.Forms.Label lbLastUpdateValue;
        private System.Windows.Forms.Label lbLastUpdate;
        private System.Windows.Forms.Label lbInPage;
        private System.Windows.Forms.Label lbInPageValue;
        private System.Windows.Forms.ComboBox cbCategory;
    }
}

