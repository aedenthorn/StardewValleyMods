namespace ContentPackCreator
{
    partial class ContentPack
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
            this.tabControl = new System.Windows.Forms.TabControl();
            this.manifestTab = new System.Windows.Forms.TabPage();
            this.tabControl2 = new System.Windows.Forms.TabControl();
            this.mainTab = new System.Windows.Forms.TabPage();
            this.minText = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.idText = new System.Windows.Forms.TextBox();
            this.descText = new System.Windows.Forms.TextBox();
            this.versionText = new System.Windows.Forms.TextBox();
            this.authorText = new System.Windows.Forms.TextBox();
            this.nameText = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.forMin = new System.Windows.Forms.TextBox();
            this.forID = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.depTab = new System.Windows.Forms.TabPage();
            this.depTable = new System.Windows.Forms.TableLayoutPanel();
            this.addDepButton = new System.Windows.Forms.Button();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.contentTab = new System.Windows.Forms.TabPage();
            this.label9 = new System.Windows.Forms.Label();
            this.folderText = new System.Windows.Forms.TextBox();
            this.buildButton = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tabControl.SuspendLayout();
            this.manifestTab.SuspendLayout();
            this.tabControl2.SuspendLayout();
            this.mainTab.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.depTab.SuspendLayout();
            this.depTable.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl
            // 
            this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl.Controls.Add(this.manifestTab);
            this.tabControl.Controls.Add(this.contentTab);
            this.tabControl.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.tabControl.Location = new System.Drawing.Point(12, 12);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(1218, 649);
            this.tabControl.TabIndex = 0;
            this.tabControl.SelectedIndexChanged += new System.EventHandler(this.tabControl_SelectedIndexChanged);
            // 
            // manifestTab
            // 
            this.manifestTab.AutoScroll = true;
            this.manifestTab.BackColor = System.Drawing.Color.DimGray;
            this.manifestTab.Controls.Add(this.tabControl2);
            this.manifestTab.Location = new System.Drawing.Point(4, 30);
            this.manifestTab.Name = "manifestTab";
            this.manifestTab.Padding = new System.Windows.Forms.Padding(3);
            this.manifestTab.Size = new System.Drawing.Size(1210, 615);
            this.manifestTab.TabIndex = 0;
            this.manifestTab.Text = "manifest.json";
            // 
            // tabControl2
            // 
            this.tabControl2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl2.Controls.Add(this.mainTab);
            this.tabControl2.Controls.Add(this.tabPage2);
            this.tabControl2.Controls.Add(this.depTab);
            this.tabControl2.Controls.Add(this.tabPage4);
            this.tabControl2.Location = new System.Drawing.Point(6, 6);
            this.tabControl2.Name = "tabControl2";
            this.tabControl2.SelectedIndex = 0;
            this.tabControl2.Size = new System.Drawing.Size(1197, 603);
            this.tabControl2.TabIndex = 0;
            // 
            // mainTab
            // 
            this.mainTab.BackColor = System.Drawing.Color.Gray;
            this.mainTab.Controls.Add(this.minText);
            this.mainTab.Controls.Add(this.label6);
            this.mainTab.Controls.Add(this.idText);
            this.mainTab.Controls.Add(this.descText);
            this.mainTab.Controls.Add(this.versionText);
            this.mainTab.Controls.Add(this.authorText);
            this.mainTab.Controls.Add(this.nameText);
            this.mainTab.Controls.Add(this.label5);
            this.mainTab.Controls.Add(this.label4);
            this.mainTab.Controls.Add(this.label3);
            this.mainTab.Controls.Add(this.label2);
            this.mainTab.Controls.Add(this.label1);
            this.mainTab.Location = new System.Drawing.Point(4, 30);
            this.mainTab.Name = "mainTab";
            this.mainTab.Padding = new System.Windows.Forms.Padding(3);
            this.mainTab.Size = new System.Drawing.Size(1189, 569);
            this.mainTab.TabIndex = 0;
            this.mainTab.Text = "Main";
            // 
            // minText
            // 
            this.minText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.minText.BackColor = System.Drawing.SystemColors.ControlDark;
            this.minText.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.minText.Location = new System.Drawing.Point(150, 183);
            this.minText.Name = "minText";
            this.minText.Size = new System.Drawing.Size(1033, 29);
            this.minText.TabIndex = 28;
            // 
            // label6
            // 
            this.label6.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label6.Location = new System.Drawing.Point(6, 183);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(138, 30);
            this.label6.TabIndex = 27;
            this.label6.Text = "MinimumApiVersion";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // idText
            // 
            this.idText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.idText.BackColor = System.Drawing.SystemColors.ControlDark;
            this.idText.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.idText.Location = new System.Drawing.Point(150, 147);
            this.idText.Name = "idText";
            this.idText.Size = new System.Drawing.Size(1033, 29);
            this.idText.TabIndex = 26;
            // 
            // descText
            // 
            this.descText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.descText.BackColor = System.Drawing.SystemColors.ControlDark;
            this.descText.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.descText.Location = new System.Drawing.Point(150, 111);
            this.descText.Name = "descText";
            this.descText.Size = new System.Drawing.Size(1033, 29);
            this.descText.TabIndex = 25;
            // 
            // versionText
            // 
            this.versionText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.versionText.BackColor = System.Drawing.SystemColors.ControlDark;
            this.versionText.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.versionText.Location = new System.Drawing.Point(150, 75);
            this.versionText.Name = "versionText";
            this.versionText.Size = new System.Drawing.Size(1033, 29);
            this.versionText.TabIndex = 24;
            // 
            // authorText
            // 
            this.authorText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.authorText.BackColor = System.Drawing.SystemColors.ControlDark;
            this.authorText.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.authorText.Location = new System.Drawing.Point(150, 39);
            this.authorText.Name = "authorText";
            this.authorText.Size = new System.Drawing.Size(1033, 29);
            this.authorText.TabIndex = 23;
            // 
            // nameText
            // 
            this.nameText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nameText.BackColor = System.Drawing.SystemColors.ControlDark;
            this.nameText.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.nameText.Location = new System.Drawing.Point(150, 3);
            this.nameText.Name = "nameText";
            this.nameText.Size = new System.Drawing.Size(1033, 29);
            this.nameText.TabIndex = 14;
            // 
            // label5
            // 
            this.label5.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label5.Location = new System.Drawing.Point(6, 147);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(138, 30);
            this.label5.TabIndex = 19;
            this.label5.Text = "UniqueID";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label4
            // 
            this.label4.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label4.Location = new System.Drawing.Point(6, 110);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(138, 30);
            this.label4.TabIndex = 18;
            this.label4.Text = "Description";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label3.Location = new System.Drawing.Point(6, 74);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(138, 30);
            this.label3.TabIndex = 17;
            this.label3.Text = "Version";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label2.Location = new System.Drawing.Point(6, 39);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(138, 30);
            this.label2.TabIndex = 16;
            this.label2.Text = "Author";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label1.Location = new System.Drawing.Point(6, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(138, 30);
            this.label1.TabIndex = 15;
            this.label1.Text = "Name";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.forMin);
            this.tabPage2.Controls.Add(this.forID);
            this.tabPage2.Controls.Add(this.label8);
            this.tabPage2.Controls.Add(this.label7);
            this.tabPage2.Location = new System.Drawing.Point(4, 30);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(1189, 569);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "ContentPackFor";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // forMin
            // 
            this.forMin.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.forMin.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.forMin.Location = new System.Drawing.Point(130, 39);
            this.forMin.MinimumSize = new System.Drawing.Size(612, 30);
            this.forMin.Name = "forMin";
            this.forMin.Size = new System.Drawing.Size(1053, 29);
            this.forMin.TabIndex = 35;
            this.forMin.Text = "1.25.0";
            // 
            // forID
            // 
            this.forID.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.forID.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.forID.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.forID.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.forID.Location = new System.Drawing.Point(130, 6);
            this.forID.MinimumSize = new System.Drawing.Size(612, 30);
            this.forID.Name = "forID";
            this.forID.Size = new System.Drawing.Size(1053, 29);
            this.forID.TabIndex = 34;
            this.forID.Text = "Pathoschild.ContentPatcher";
            // 
            // label8
            // 
            this.label8.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label8.Location = new System.Drawing.Point(9, 39);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(135, 30);
            this.label8.TabIndex = 31;
            this.label8.Text = "MinimumVersion";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label7
            // 
            this.label7.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label7.Location = new System.Drawing.Point(9, 6);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(135, 30);
            this.label7.TabIndex = 29;
            this.label7.Text = "UniqueID";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // depTab
            // 
            this.depTab.Controls.Add(this.depTable);
            this.depTab.Location = new System.Drawing.Point(4, 30);
            this.depTab.Name = "depTab";
            this.depTab.Padding = new System.Windows.Forms.Padding(3);
            this.depTab.Size = new System.Drawing.Size(1189, 569);
            this.depTab.TabIndex = 2;
            this.depTab.Text = "Dependencies";
            this.depTab.UseVisualStyleBackColor = true;
            // 
            // depTable
            // 
            this.depTable.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.depTable.AutoScroll = true;
            this.depTable.ColumnCount = 1;
            this.depTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.depTable.Controls.Add(this.addDepButton, 0, 0);
            this.depTable.Location = new System.Drawing.Point(6, 6);
            this.depTable.Name = "depTable";
            this.depTable.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.depTable.RowCount = 1;
            this.depTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 569F));
            this.depTable.Size = new System.Drawing.Size(1177, 569);
            this.depTable.TabIndex = 0;
            // 
            // addDepButton
            // 
            this.addDepButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.addDepButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.addDepButton.Location = new System.Drawing.Point(13, 3);
            this.addDepButton.Name = "addDepButton";
            this.addDepButton.Size = new System.Drawing.Size(1151, 29);
            this.addDepButton.TabIndex = 1;
            this.addDepButton.Text = "Add Dependency";
            this.addDepButton.UseVisualStyleBackColor = true;
            this.addDepButton.Click += new System.EventHandler(this.addDepButton_Click);
            // 
            // tabPage4
            // 
            this.tabPage4.Location = new System.Drawing.Point(4, 30);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage4.Size = new System.Drawing.Size(1189, 569);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "Extra";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // contentTab
            // 
            this.contentTab.BackColor = System.Drawing.Color.Gray;
            this.contentTab.Location = new System.Drawing.Point(4, 30);
            this.contentTab.Name = "contentTab";
            this.contentTab.Padding = new System.Windows.Forms.Padding(3);
            this.contentTab.Size = new System.Drawing.Size(1210, 615);
            this.contentTab.TabIndex = 1;
            this.contentTab.Text = "content.json";
            // 
            // label9
            // 
            this.label9.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label9.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label9.Location = new System.Drawing.Point(12, 683);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(90, 30);
            this.label9.TabIndex = 19;
            this.label9.Text = "Folder Name";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // folderText
            // 
            this.folderText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.folderText.BackColor = System.Drawing.SystemColors.ControlDark;
            this.folderText.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.folderText.Location = new System.Drawing.Point(108, 683);
            this.folderText.Name = "folderText";
            this.folderText.Size = new System.Drawing.Size(956, 29);
            this.folderText.TabIndex = 17;
            // 
            // buildButton
            // 
            this.buildButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buildButton.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.buildButton.Location = new System.Drawing.Point(1070, 683);
            this.buildButton.Name = "buildButton";
            this.buildButton.Size = new System.Drawing.Size(156, 30);
            this.buildButton.TabIndex = 18;
            this.buildButton.Text = "Build";
            this.buildButton.UseVisualStyleBackColor = true;
            this.buildButton.Click += new System.EventHandler(this.buildButton_Click);
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.button1.Location = new System.Drawing.Point(13, 3);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(174, 29);
            this.button1.TabIndex = 1;
            this.button1.Text = "Add Dependency";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.addDepButton_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.AutoScroll = true;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.Size = new System.Drawing.Size(200, 100);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // ContentPack
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.ClientSize = new System.Drawing.Size(1242, 726);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.folderText);
            this.Controls.Add(this.buildButton);
            this.Controls.Add(this.tabControl);
            this.Name = "ContentPack";
            this.Text = "ContentPack";
            this.tabControl.ResumeLayout(false);
            this.manifestTab.ResumeLayout(false);
            this.tabControl2.ResumeLayout(false);
            this.mainTab.ResumeLayout(false);
            this.mainTab.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.depTab.ResumeLayout(false);
            this.depTable.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage manifestTab;
        private System.Windows.Forms.TabPage contentTab;
        private System.Windows.Forms.TabControl tabControl2;
        private System.Windows.Forms.TabPage mainTab;
        private System.Windows.Forms.TextBox minText;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox idText;
        private System.Windows.Forms.TextBox descText;
        private System.Windows.Forms.TextBox versionText;
        private System.Windows.Forms.TextBox authorText;
        private System.Windows.Forms.TextBox nameText;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TextBox forMin;
        private System.Windows.Forms.TextBox forID;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TabPage depTab;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.TableLayoutPanel depTable;
        private System.Windows.Forms.Button addDepButton;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox folderText;
        private System.Windows.Forms.Button buildButton;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}