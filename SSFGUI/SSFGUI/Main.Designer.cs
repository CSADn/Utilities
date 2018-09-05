namespace SSFGUI
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.btClient = new System.Windows.Forms.Button();
            this.btServer = new System.Windows.Forms.Button();
            this.tbIPAddress = new System.Windows.Forms.TextBox();
            this.cbVerify = new System.Windows.Forms.CheckBox();
            this.tooltip = new System.Windows.Forms.ToolTip(this.components);
            this.cbDebug = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // btClient
            // 
            this.btClient.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btClient.Location = new System.Drawing.Point(12, 12);
            this.btClient.Name = "btClient";
            this.btClient.Size = new System.Drawing.Size(177, 68);
            this.btClient.TabIndex = 0;
            this.btClient.Text = "#1 Cliente";
            this.btClient.UseVisualStyleBackColor = true;
            // 
            // btServer
            // 
            this.btServer.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btServer.Location = new System.Drawing.Point(195, 12);
            this.btServer.Name = "btServer";
            this.btServer.Size = new System.Drawing.Size(177, 68);
            this.btServer.TabIndex = 1;
            this.btServer.Text = "#2 Servidor";
            this.btServer.UseVisualStyleBackColor = true;
            // 
            // tbIPAddress
            // 
            this.tbIPAddress.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbIPAddress.Location = new System.Drawing.Point(195, 87);
            this.tbIPAddress.Name = "tbIPAddress";
            this.tbIPAddress.Size = new System.Drawing.Size(177, 29);
            this.tbIPAddress.TabIndex = 2;
            this.tbIPAddress.Text = "192.168.0.100";
            this.tbIPAddress.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // cbVerify
            // 
            this.cbVerify.AutoSize = true;
            this.cbVerify.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.cbVerify.Checked = true;
            this.cbVerify.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbVerify.Location = new System.Drawing.Point(221, 122);
            this.cbVerify.Name = "cbVerify";
            this.cbVerify.Size = new System.Drawing.Size(153, 17);
            this.cbVerify.TabIndex = 3;
            this.cbVerify.Text = "Intento de conexión infinito";
            this.cbVerify.UseVisualStyleBackColor = true;
            // 
            // tooltip
            // 
            this.tooltip.AutoPopDelay = 10000;
            this.tooltip.InitialDelay = 500;
            this.tooltip.IsBalloon = true;
            this.tooltip.ReshowDelay = 100;
            // 
            // cbDebug
            // 
            this.cbDebug.AutoSize = true;
            this.cbDebug.Location = new System.Drawing.Point(12, 122);
            this.cbDebug.Name = "cbDebug";
            this.cbDebug.Size = new System.Drawing.Size(58, 17);
            this.cbDebug.TabIndex = 4;
            this.cbDebug.Text = "Debug";
            this.cbDebug.UseVisualStyleBackColor = true;
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(386, 147);
            this.Controls.Add(this.cbDebug);
            this.Controls.Add(this.cbVerify);
            this.Controls.Add(this.tbIPAddress);
            this.Controls.Add(this.btServer);
            this.Controls.Add(this.btClient);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "Main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "SSF - GUI";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btClient;
        private System.Windows.Forms.Button btServer;
        private System.Windows.Forms.TextBox tbIPAddress;
        private System.Windows.Forms.CheckBox cbVerify;
        private System.Windows.Forms.ToolTip tooltip;
        private System.Windows.Forms.CheckBox cbDebug;
    }
}

