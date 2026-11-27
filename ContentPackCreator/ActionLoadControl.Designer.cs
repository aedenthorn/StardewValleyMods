namespace ContentPackCreator
{
    partial class ActionLoadControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ActionLoadControl));
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.label3 = new System.Windows.Forms.Label();
            this.targetText = new System.Windows.Forms.TextBox();
            this.fromFileText = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.cpActionTabControl = new System.Windows.Forms.TabControl();
            this.mainTab = new System.Windows.Forms.TabPage();
            this.whenTab = new System.Windows.Forms.TabPage();
            this.addWhenButton = new System.Windows.Forms.Button();
            this.whenTable = new System.Windows.Forms.TableLayoutPanel();
            this.cpActionTabControl.SuspendLayout();
            this.mainTab.SuspendLayout();
            this.whenTab.SuspendLayout();
            this.whenTable.SuspendLayout();
            this.SuspendLayout();
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label3.Location = new System.Drawing.Point(6, 3);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(122, 30);
            this.label3.TabIndex = 62;
            this.label3.Text = "Target";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.toolTip1.SetToolTip(this.label3, "The game asset name to replace (or multiple comma-delimited asset names), like Po" +
        "rtraits/Abigail. This field supports tokens, and capitalisation doesn\'t matter.");
            // 
            // targetText
            // 
            this.targetText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.targetText.BackColor = System.Drawing.SystemColors.ControlDark;
            this.targetText.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.targetText.Location = new System.Drawing.Point(154, 3);
            this.targetText.Name = "targetText";
            this.targetText.Size = new System.Drawing.Size(883, 29);
            this.targetText.TabIndex = 61;
            this.toolTip1.SetToolTip(this.targetText, "The game asset name to replace (or multiple comma-delimited asset names), like Po" +
        "rtraits/Abigail. This field supports tokens, and capitalisation doesn\'t matter.");
            // 
            // fromFileText
            // 
            this.fromFileText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.fromFileText.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.fromFileText.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.fromFileText.BackColor = System.Drawing.SystemColors.ControlDark;
            this.fromFileText.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.fromFileText.Location = new System.Drawing.Point(154, 39);
            this.fromFileText.Name = "fromFileText";
            this.fromFileText.Size = new System.Drawing.Size(883, 29);
            this.fromFileText.TabIndex = 63;
            this.toolTip1.SetToolTip(this.fromFileText, resources.GetString("fromFileText.ToolTip"));
            // 
            // label4
            // 
            this.label4.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label4.Location = new System.Drawing.Point(6, 38);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(122, 30);
            this.label4.TabIndex = 60;
            this.label4.Text = "FromFile";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.toolTip1.SetToolTip(this.label4, resources.GetString("label4.ToolTip"));
            // 
            // cpActionTabControl
            // 
            this.cpActionTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cpActionTabControl.Controls.Add(this.mainTab);
            this.cpActionTabControl.Controls.Add(this.whenTab);
            this.cpActionTabControl.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.cpActionTabControl.Location = new System.Drawing.Point(3, 3);
            this.cpActionTabControl.Name = "cpActionTabControl";
            this.cpActionTabControl.SelectedIndex = 0;
            this.cpActionTabControl.Size = new System.Drawing.Size(1051, 527);
            this.cpActionTabControl.TabIndex = 67;
            // 
            // mainTab
            // 
            this.mainTab.BackColor = System.Drawing.Color.Gray;
            this.mainTab.Controls.Add(this.label3);
            this.mainTab.Controls.Add(this.label4);
            this.mainTab.Controls.Add(this.targetText);
            this.mainTab.Controls.Add(this.fromFileText);
            this.mainTab.Location = new System.Drawing.Point(4, 30);
            this.mainTab.Name = "mainTab";
            this.mainTab.Padding = new System.Windows.Forms.Padding(3);
            this.mainTab.Size = new System.Drawing.Size(1043, 493);
            this.mainTab.TabIndex = 0;
            this.mainTab.Text = "Main";
            // 
            // whenTab
            // 
            this.whenTab.Controls.Add(this.whenTable);
            this.whenTab.Location = new System.Drawing.Point(4, 30);
            this.whenTab.Name = "whenTab";
            this.whenTab.Padding = new System.Windows.Forms.Padding(3);
            this.whenTab.Size = new System.Drawing.Size(1043, 493);
            this.whenTab.TabIndex = 1;
            this.whenTab.Text = "When";
            this.whenTab.UseVisualStyleBackColor = true;
            // 
            // addWhenButton
            // 
            this.addWhenButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.addWhenButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.addWhenButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.addWhenButton.Location = new System.Drawing.Point(3, 3);
            this.addWhenButton.Name = "addWhenButton";
            this.addWhenButton.Size = new System.Drawing.Size(1031, 29);
            this.addWhenButton.TabIndex = 2;
            this.addWhenButton.Text = "Add When Condition";
            this.addWhenButton.UseVisualStyleBackColor = true;
            this.addWhenButton.Click += new System.EventHandler(this.addWhenButton_Click);
            // 
            // whenTable
            // 
            this.whenTable.BackColor = System.Drawing.Color.DimGray;
            this.whenTable.ColumnCount = 1;
            this.whenTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.whenTable.Controls.Add(this.addWhenButton, 0, 0);
            this.whenTable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.whenTable.Location = new System.Drawing.Point(3, 3);
            this.whenTable.Name = "whenTable";
            this.whenTable.RowCount = 1;
            this.whenTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 487F));
            this.whenTable.Size = new System.Drawing.Size(1037, 487);
            this.whenTable.TabIndex = 0;
            // 
            // ActionLoadControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.Controls.Add(this.cpActionTabControl);
            this.Name = "ActionLoadControl";
            this.Size = new System.Drawing.Size(1057, 533);
            this.cpActionTabControl.ResumeLayout(false);
            this.mainTab.ResumeLayout(false);
            this.mainTab.PerformLayout();
            this.whenTab.ResumeLayout(false);
            this.whenTable.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox targetText;
        private System.Windows.Forms.TextBox fromFileText;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TabControl cpActionTabControl;
        private System.Windows.Forms.TabPage mainTab;
        private System.Windows.Forms.TabPage whenTab;
        private System.Windows.Forms.TableLayoutPanel whenTable;
        private System.Windows.Forms.Button addWhenButton;
    }
}
