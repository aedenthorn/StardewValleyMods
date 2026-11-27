namespace ContentPackCreator
{
    partial class DepControl
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
            this.depMin = new System.Windows.Forms.TextBox();
            this.depID = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.depReq = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // depMin
            // 
            this.depMin.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.depMin.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.depMin.Location = new System.Drawing.Point(124, 36);
            this.depMin.Name = "depMin";
            this.depMin.Size = new System.Drawing.Size(568, 29);
            this.depMin.TabIndex = 39;
            // 
            // depID
            // 
            this.depID.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.depID.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.depID.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.depID.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.depID.Location = new System.Drawing.Point(124, 3);
            this.depID.Name = "depID";
            this.depID.Size = new System.Drawing.Size(568, 29);
            this.depID.TabIndex = 38;
            // 
            // label8
            // 
            this.label8.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label8.Location = new System.Drawing.Point(0, 36);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(135, 30);
            this.label8.TabIndex = 37;
            this.label8.Text = "MinimumVersion";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label7
            // 
            this.label7.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label7.Location = new System.Drawing.Point(0, 3);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(135, 30);
            this.label7.TabIndex = 36;
            this.label7.Text = "UniqueID";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label1.Location = new System.Drawing.Point(0, 69);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(135, 30);
            this.label1.TabIndex = 40;
            this.label1.Text = "IsRequired";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // depReq
            // 
            this.depReq.AutoSize = true;
            this.depReq.Checked = true;
            this.depReq.CheckState = System.Windows.Forms.CheckState.Checked;
            this.depReq.Location = new System.Drawing.Point(124, 77);
            this.depReq.Name = "depReq";
            this.depReq.Size = new System.Drawing.Size(48, 19);
            this.depReq.TabIndex = 41;
            this.depReq.Text = "True";
            this.depReq.UseVisualStyleBackColor = true;
            this.depReq.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // label2
            // 
            this.label2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label2.Location = new System.Drawing.Point(3, 108);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(689, 2);
            this.label2.TabIndex = 42;
            this.label2.Text = "label2";
            // 
            // DepControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label2);
            this.Controls.Add(this.depReq);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.depMin);
            this.Controls.Add(this.depID);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Name = "DepControl";
            this.Size = new System.Drawing.Size(695, 120);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox depMin;
        private System.Windows.Forms.TextBox depID;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox depReq;
        private System.Windows.Forms.Label label2;
    }
}
