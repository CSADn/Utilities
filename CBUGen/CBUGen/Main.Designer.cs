namespace CBUGen
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
            this.Bancos = new System.Windows.Forms.ComboBox();
            this.CBU = new System.Windows.Forms.TextBox();
            this.Generar = new System.Windows.Forms.Button();
            this.Copiar = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // Bancos
            // 
            this.Bancos.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.Bancos.FormattingEnabled = true;
            this.Bancos.Location = new System.Drawing.Point(12, 31);
            this.Bancos.Name = "Bancos";
            this.Bancos.Size = new System.Drawing.Size(471, 21);
            this.Bancos.TabIndex = 0;
            // 
            // CBU
            // 
            this.CBU.Font = new System.Drawing.Font("Courier New", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CBU.Location = new System.Drawing.Point(12, 68);
            this.CBU.Name = "CBU";
            this.CBU.ReadOnly = true;
            this.CBU.Size = new System.Drawing.Size(470, 29);
            this.CBU.TabIndex = 1;
            this.CBU.Text = "0000000000000000000000";
            this.CBU.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // Generar
            // 
            this.Generar.Location = new System.Drawing.Point(407, 107);
            this.Generar.Name = "Generar";
            this.Generar.Size = new System.Drawing.Size(75, 23);
            this.Generar.TabIndex = 2;
            this.Generar.Text = "Generar";
            this.Generar.UseVisualStyleBackColor = true;
            this.Generar.Click += new System.EventHandler(this.Generar_Click);
            // 
            // Copiar
            // 
            this.Copiar.Location = new System.Drawing.Point(326, 107);
            this.Copiar.Name = "Copiar";
            this.Copiar.Size = new System.Drawing.Size(75, 23);
            this.Copiar.TabIndex = 3;
            this.Copiar.Text = "Copiar";
            this.Copiar.UseVisualStyleBackColor = true;
            this.Copiar.Click += new System.EventHandler(this.Copiar_Click);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(494, 152);
            this.Controls.Add(this.Copiar);
            this.Controls.Add(this.Generar);
            this.Controls.Add(this.CBU);
            this.Controls.Add(this.Bancos);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "Main";
            this.Text = "Generador de CBU";
            this.Load += new System.EventHandler(this.Main_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox Bancos;
        private System.Windows.Forms.TextBox CBU;
        private System.Windows.Forms.Button Generar;
        private System.Windows.Forms.Button Copiar;
    }
}

