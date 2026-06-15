namespace SSPVEncryptDecrypt
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
            this.Encrypt = new System.Windows.Forms.Button();
            this.SourceEncrypt = new System.Windows.Forms.TextBox();
            this.TargetEncrypt = new System.Windows.Forms.TextBox();
            this.TargetDecrypt = new System.Windows.Forms.TextBox();
            this.SourceDecrypt = new System.Windows.Forms.TextBox();
            this.Decrypt = new System.Windows.Forms.Button();
            this.SaltEncrypt = new System.Windows.Forms.TextBox();
            this.SaltDecrypt = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // Encrypt
            // 
            this.Encrypt.Location = new System.Drawing.Point(357, 12);
            this.Encrypt.Name = "Encrypt";
            this.Encrypt.Size = new System.Drawing.Size(75, 23);
            this.Encrypt.TabIndex = 2;
            this.Encrypt.Text = "Encrypt";
            this.Encrypt.UseVisualStyleBackColor = true;
            this.Encrypt.Click += new System.EventHandler(this.Encrypt_Click);
            // 
            // SourceEncrypt
            // 
            this.SourceEncrypt.Location = new System.Drawing.Point(12, 12);
            this.SourceEncrypt.Name = "SourceEncrypt";
            this.SourceEncrypt.Size = new System.Drawing.Size(186, 20);
            this.SourceEncrypt.TabIndex = 0;
            this.SourceEncrypt.Text = "contraseña";
            // 
            // TargetEncrypt
            // 
            this.TargetEncrypt.Location = new System.Drawing.Point(438, 12);
            this.TargetEncrypt.Name = "TargetEncrypt";
            this.TargetEncrypt.Size = new System.Drawing.Size(186, 20);
            this.TargetEncrypt.TabIndex = 3;
            // 
            // TargetDecrypt
            // 
            this.TargetDecrypt.Location = new System.Drawing.Point(438, 41);
            this.TargetDecrypt.Name = "TargetDecrypt";
            this.TargetDecrypt.Size = new System.Drawing.Size(186, 20);
            this.TargetDecrypt.TabIndex = 7;
            // 
            // SourceDecrypt
            // 
            this.SourceDecrypt.Location = new System.Drawing.Point(12, 41);
            this.SourceDecrypt.Name = "SourceDecrypt";
            this.SourceDecrypt.Size = new System.Drawing.Size(186, 20);
            this.SourceDecrypt.TabIndex = 4;
            this.SourceDecrypt.Text = "hash";
            // 
            // Decrypt
            // 
            this.Decrypt.Location = new System.Drawing.Point(357, 41);
            this.Decrypt.Name = "Decrypt";
            this.Decrypt.Size = new System.Drawing.Size(75, 23);
            this.Decrypt.TabIndex = 6;
            this.Decrypt.Text = "Decrypt";
            this.Decrypt.UseVisualStyleBackColor = true;
            this.Decrypt.Click += new System.EventHandler(this.Decrypt_Click);
            // 
            // SaltEncrypt
            // 
            this.SaltEncrypt.Location = new System.Drawing.Point(204, 12);
            this.SaltEncrypt.Name = "SaltEncrypt";
            this.SaltEncrypt.Size = new System.Drawing.Size(147, 20);
            this.SaltEncrypt.TabIndex = 1;
            this.SaltEncrypt.Text = "nombreusuario";
            // 
            // SaltDecrypt
            // 
            this.SaltDecrypt.Location = new System.Drawing.Point(204, 41);
            this.SaltDecrypt.Name = "SaltDecrypt";
            this.SaltDecrypt.Size = new System.Drawing.Size(147, 20);
            this.SaltDecrypt.TabIndex = 5;
            this.SaltDecrypt.Text = "nombreusuario";
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(637, 76);
            this.Controls.Add(this.SaltDecrypt);
            this.Controls.Add(this.SaltEncrypt);
            this.Controls.Add(this.TargetDecrypt);
            this.Controls.Add(this.SourceDecrypt);
            this.Controls.Add(this.Decrypt);
            this.Controls.Add(this.TargetEncrypt);
            this.Controls.Add(this.SourceEncrypt);
            this.Controls.Add(this.Encrypt);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "Main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "SSPV Encrypt - Decrypt";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Encrypt;
        private System.Windows.Forms.TextBox SourceEncrypt;
        private System.Windows.Forms.TextBox TargetEncrypt;
        private System.Windows.Forms.TextBox TargetDecrypt;
        private System.Windows.Forms.TextBox SourceDecrypt;
        private System.Windows.Forms.Button Decrypt;
        private System.Windows.Forms.TextBox SaltEncrypt;
        private System.Windows.Forms.TextBox SaltDecrypt;
    }
}

