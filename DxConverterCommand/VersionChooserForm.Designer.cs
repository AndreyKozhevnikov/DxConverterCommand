namespace DxConverterCommand {
    partial class VersionChooserForm {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.cbVersion = new System.Windows.Forms.ComboBox();
            this.btnConvert = new System.Windows.Forms.Button();
            this.tbSolutionPath = new System.Windows.Forms.TextBox();
            this.btnUpdate = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // cbVersion
            // 
            this.cbVersion.FormattingEnabled = true;
            this.cbVersion.Location = new System.Drawing.Point(12, 3);
            this.cbVersion.Name = "cbVersion";
            this.cbVersion.Size = new System.Drawing.Size(268, 21);
            this.cbVersion.TabIndex = 0;
            // 
            // btnConvert
            // 
            this.btnConvert.Location = new System.Drawing.Point(286, 3);
            this.btnConvert.Name = "btnConvert";
            this.btnConvert.Size = new System.Drawing.Size(138, 40);
            this.btnConvert.TabIndex = 1;
            this.btnConvert.Text = "Run converter";
            this.btnConvert.UseVisualStyleBackColor = true;
            // 
            // tbSolutionPath
            // 
            this.tbSolutionPath.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tbSolutionPath.Location = new System.Drawing.Point(12, 30);
            this.tbSolutionPath.Name = "tbSolutionPath";
            this.tbSolutionPath.ReadOnly = true;
            this.tbSolutionPath.Size = new System.Drawing.Size(268, 13);
            this.tbSolutionPath.TabIndex = 2;
            // 
            // btnUpdate
            // 
            this.btnUpdate.Location = new System.Drawing.Point(12, 51);
            this.btnUpdate.Name = "btnUpdate";
            this.btnUpdate.Size = new System.Drawing.Size(127, 23);
            this.btnUpdate.TabIndex = 3;
            this.btnUpdate.Text = "update versions";
            this.btnUpdate.UseVisualStyleBackColor = true;
            // 
            // VersionChooserForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(440, 86);
            this.Controls.Add(this.btnUpdate);
            this.Controls.Add(this.tbSolutionPath);
            this.Controls.Add(this.btnConvert);
            this.Controls.Add(this.cbVersion);
            this.Name = "VersionChooserForm";
            this.Text = "VersionChooserForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cbVersion;
        private System.Windows.Forms.Button btnConvert;
        private System.Windows.Forms.TextBox tbSolutionPath;
        private System.Windows.Forms.Button btnUpdate;
    }
}