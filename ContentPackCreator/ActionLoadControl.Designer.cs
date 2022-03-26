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
            this.label3 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.mainTab = new System.Windows.Forms.TabPage();
            this.whenTab = new System.Windows.Forms.TabPage();
            this.whenTable = new System.Windows.Forms.TableLayoutPanel();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.addWhenButton = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
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
            this.label3.TabIndex = 58;
            this.label3.Text = "Target";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.toolTip1.SetToolTip(this.label3, "The game asset name to replace (or multiple comma-delimited asset names), like Po" +
        "rtraits/Abigail. This field supports tokens, and capitalisation doesn\'t matter.");
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.textBox1.Location = new System.Drawing.Point(134, 39);
            this.textBox1.MinimumSize = new System.Drawing.Size(612, 30);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(784, 29);
            this.textBox1.TabIndex = 59;
            this.toolTip1.SetToolTip(this.textBox1, resources.GetString("textBox1.ToolTip"));
            // 
            // label4
            // 
            this.label4.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label4.Location = new System.Drawing.Point(6, 39);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(122, 30);
            this.label4.TabIndex = 56;
            this.label4.Text = "FromFile";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.toolTip1.SetToolTip(this.label4, resources.GetString("label4.ToolTip"));
            // 
            // textBox2
            // 
            this.textBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox2.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.textBox2.Location = new System.Drawing.Point(134, 3);
            this.textBox2.MinimumSize = new System.Drawing.Size(612, 30);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(784, 29);
            this.textBox2.TabIndex = 57;
            this.toolTip1.SetToolTip(this.textBox2, "The game asset name to replace (or multiple comma-delimited asset names), like Po" +
        "rtraits/Abigail. This field supports tokens, and capitalisation doesn\'t matter.");
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.mainTab);
            this.tabControl1.Controls.Add(this.whenTab);
            this.tabControl1.Location = new System.Drawing.Point(3, 3);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(932, 333);
            this.tabControl1.TabIndex = 62;
            // 
            // mainTab
            // 
            this.mainTab.Controls.Add(this.label3);
            this.mainTab.Controls.Add(this.textBox2);
            this.mainTab.Controls.Add(this.textBox1);
            this.mainTab.Controls.Add(this.label4);
            this.mainTab.Location = new System.Drawing.Point(4, 24);
            this.mainTab.Name = "mainTab";
            this.mainTab.Padding = new System.Windows.Forms.Padding(3);
            this.mainTab.Size = new System.Drawing.Size(924, 305);
            this.mainTab.TabIndex = 0;
            this.mainTab.Text = "Main";
            this.mainTab.UseVisualStyleBackColor = true;
            // 
            // whenTab
            // 
            this.whenTab.Controls.Add(this.whenTable);
            this.whenTab.Location = new System.Drawing.Point(4, 24);
            this.whenTab.Name = "whenTab";
            this.whenTab.Padding = new System.Windows.Forms.Padding(3);
            this.whenTab.Size = new System.Drawing.Size(924, 305);
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
            this.whenTable.Size = new System.Drawing.Size(912, 293);
            this.whenTable.TabIndex = 0;
            // 
            // addWhenButton
            // 
            this.addWhenButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.addWhenButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.addWhenButton.Location = new System.Drawing.Point(3, 3);
            this.addWhenButton.Name = "addWhenButton";
            this.addWhenButton.Size = new System.Drawing.Size(906, 29);
            this.addWhenButton.TabIndex = 2;
            this.addWhenButton.Text = "Add When Condition";
            this.addWhenButton.UseVisualStyleBackColor = true;
            this.addWhenButton.Click += new System.EventHandler(this.addWhenButton_Click);
            // 
            // ActionLoadControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabControl1);
            this.Name = "ActionLoadControl";
            this.Size = new System.Drawing.Size(938, 339);
            this.tabControl1.ResumeLayout(false);
            this.mainTab.ResumeLayout(false);
            this.mainTab.PerformLayout();
            this.whenTab.ResumeLayout(false);
            this.whenTable.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage mainTab;
        private System.Windows.Forms.TabPage whenTab;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.TableLayoutPanel whenTable;
        private System.Windows.Forms.Button addWhenButton;
    }
}
