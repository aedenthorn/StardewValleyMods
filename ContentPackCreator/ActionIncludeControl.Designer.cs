namespace ContentPackCreator
{
    partial class ActionIncludeControl
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
            this.cpActionTabControl = new System.Windows.Forms.TabControl();
            this.mainTab = new System.Windows.Forms.TabPage();
            this.label3 = new System.Windows.Forms.Label();
            this.whenTab = new System.Windows.Forms.TabPage();
            this.whenTable = new System.Windows.Forms.TableLayoutPanel();
            this.addWhenButton = new System.Windows.Forms.Button();
            this.targetText = new System.Windows.Forms.TextBox();
            this.cpActionTabControl.SuspendLayout();
            this.mainTab.SuspendLayout();
            this.whenTab.SuspendLayout();
            this.whenTable.SuspendLayout();
            this.SuspendLayout();
            // 
            // cpActionTabControl
            // 
            this.cpActionTabControl.Controls.Add(this.mainTab);
            this.cpActionTabControl.Controls.Add(this.whenTab);
            this.cpActionTabControl.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.cpActionTabControl.Location = new System.Drawing.Point(3, 3);
            this.cpActionTabControl.Name = "cpActionTabControl";
            this.cpActionTabControl.SelectedIndex = 0;
            this.cpActionTabControl.Size = new System.Drawing.Size(1194, 504);
            this.cpActionTabControl.TabIndex = 68;
            // 
            // mainTab
            // 
            this.mainTab.Controls.Add(this.targetText);
            this.mainTab.Controls.Add(this.label3);
            this.mainTab.Location = new System.Drawing.Point(4, 30);
            this.mainTab.Name = "mainTab";
            this.mainTab.Padding = new System.Windows.Forms.Padding(3);
            this.mainTab.Size = new System.Drawing.Size(1186, 470);
            this.mainTab.TabIndex = 0;
            this.mainTab.Text = "Main";
            this.mainTab.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label3.Location = new System.Drawing.Point(6, 5);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(122, 30);
            this.label3.TabIndex = 62;
            this.label3.Text = "Target";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // whenTab
            // 
            this.whenTab.Controls.Add(this.whenTable);
            this.whenTab.Location = new System.Drawing.Point(4, 30);
            this.whenTab.Name = "whenTab";
            this.whenTab.Padding = new System.Windows.Forms.Padding(3);
            this.whenTab.Size = new System.Drawing.Size(1186, 470);
            this.whenTab.TabIndex = 1;
            this.whenTab.Text = "When";
            this.whenTab.UseVisualStyleBackColor = true;
            // 
            // whenTable
            // 
            this.whenTable.ColumnCount = 1;
            this.whenTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.whenTable.Controls.Add(this.addWhenButton, 0, 0);
            this.whenTable.Location = new System.Drawing.Point(6, 6);
            this.whenTable.Name = "whenTable";
            this.whenTable.RowCount = 1;
            this.whenTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 293F));
            this.whenTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 293F));
            this.whenTable.Size = new System.Drawing.Size(1174, 464);
            this.whenTable.TabIndex = 0;
            // 
            // addWhenButton
            // 
            this.addWhenButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.addWhenButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.addWhenButton.Location = new System.Drawing.Point(3, 3);
            this.addWhenButton.Name = "addWhenButton";
            this.addWhenButton.Size = new System.Drawing.Size(1168, 29);
            this.addWhenButton.TabIndex = 2;
            this.addWhenButton.Text = "Add When Condition";
            this.addWhenButton.UseVisualStyleBackColor = true;
            // 
            // targetText
            // 
            this.targetText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.targetText.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.targetText.Location = new System.Drawing.Point(154, 6);
            this.targetText.Name = "targetText";
            this.targetText.Size = new System.Drawing.Size(1026, 29);
            this.targetText.TabIndex = 64;
            // 
            // ActionIncludeControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.cpActionTabControl);
            this.Name = "ActionIncludeControl";
            this.Size = new System.Drawing.Size(1200, 510);
            this.cpActionTabControl.ResumeLayout(false);
            this.mainTab.ResumeLayout(false);
            this.mainTab.PerformLayout();
            this.whenTab.ResumeLayout(false);
            this.whenTable.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl cpActionTabControl;
        private System.Windows.Forms.TabPage mainTab;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TabPage whenTab;
        private System.Windows.Forms.TableLayoutPanel whenTable;
        private System.Windows.Forms.Button addWhenButton;
        private System.Windows.Forms.TextBox targetText;
    }
}
