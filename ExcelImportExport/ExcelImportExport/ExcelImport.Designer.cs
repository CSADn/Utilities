namespace ExcelImportExport
{
    partial class ExcelImport
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExcelImport));
            this.label1 = new System.Windows.Forms.Label();
            this.FileTxt = new System.Windows.Forms.TextBox();
            this.ExitBtn = new System.Windows.Forms.Button();
            this.OpenBtn = new System.Windows.Forms.Button();
            this.StatusLbl = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 26);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(43, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Archivo";
            // 
            // FileTxt
            // 
            this.FileTxt.Location = new System.Drawing.Point(61, 23);
            this.FileTxt.Name = "FileTxt";
            this.FileTxt.ReadOnly = true;
            this.FileTxt.Size = new System.Drawing.Size(338, 20);
            this.FileTxt.TabIndex = 1;
            // 
            // ExitBtn
            // 
            this.ExitBtn.AutoSize = true;
            this.ExitBtn.Image = global::ExcelImportExport.Properties.Resources.exit_32;
            this.ExitBtn.Location = new System.Drawing.Point(397, 56);
            this.ExitBtn.Name = "ExitBtn";
            this.ExitBtn.Size = new System.Drawing.Size(38, 38);
            this.ExitBtn.TabIndex = 3;
            this.ExitBtn.UseVisualStyleBackColor = true;
            // 
            // OpenBtn
            // 
            this.OpenBtn.AutoSize = true;
            this.OpenBtn.Image = global::ExcelImportExport.Properties.Resources.open_file;
            this.OpenBtn.Location = new System.Drawing.Point(405, 17);
            this.OpenBtn.Name = "OpenBtn";
            this.OpenBtn.Size = new System.Drawing.Size(30, 30);
            this.OpenBtn.TabIndex = 2;
            this.OpenBtn.UseVisualStyleBackColor = true;
            // 
            // StatusLbl
            // 
            this.StatusLbl.Font = new System.Drawing.Font("Tahoma", 21.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.StatusLbl.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.StatusLbl.Location = new System.Drawing.Point(58, 56);
            this.StatusLbl.Name = "StatusLbl";
            this.StatusLbl.Size = new System.Drawing.Size(333, 31);
            this.StatusLbl.TabIndex = 4;
            // 
            // ExcelImport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(440, 98);
            this.Controls.Add(this.StatusLbl);
            this.Controls.Add(this.ExitBtn);
            this.Controls.Add(this.OpenBtn);
            this.Controls.Add(this.FileTxt);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ExcelImport";
            this.Text = "Importación Excel";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox FileTxt;
        private System.Windows.Forms.Button OpenBtn;
        private System.Windows.Forms.Button ExitBtn;
        private System.Windows.Forms.Label StatusLbl;
    }
}

