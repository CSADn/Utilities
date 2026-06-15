namespace WhatsappSimulator
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
            this.cbBroker = new System.Windows.Forms.ComboBox();
            this.lblPhoneTo = new System.Windows.Forms.Label();
            this.tbPhoneTo = new System.Windows.Forms.TextBox();
            this.lblBroker = new System.Windows.Forms.Label();
            this.Tabs = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tbMessageText = new System.Windows.Forms.TextBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.tabPage5 = new System.Windows.Forms.TabPage();
            this.tabPage6 = new System.Windows.Forms.TabPage();
            this.btSend = new System.Windows.Forms.Button();
            this.lblWebhook = new System.Windows.Forms.Label();
            this.cbWebhook = new System.Windows.Forms.ComboBox();
            this.lblPhoneFrom = new System.Windows.Forms.Label();
            this.cbPhoneFrom = new System.Windows.Forms.ComboBox();
            this.ssStatus = new System.Windows.Forms.StatusStrip();
            this.tsLbStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.Tabs.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.ssStatus.SuspendLayout();
            this.SuspendLayout();
            // 
            // cbBroker
            // 
            this.cbBroker.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbBroker.FormattingEnabled = true;
            this.cbBroker.Location = new System.Drawing.Point(127, 88);
            this.cbBroker.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cbBroker.Name = "cbBroker";
            this.cbBroker.Size = new System.Drawing.Size(383, 29);
            this.cbBroker.TabIndex = 2;
            // 
            // lblPhoneTo
            // 
            this.lblPhoneTo.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPhoneTo.Location = new System.Drawing.Point(13, 48);
            this.lblPhoneTo.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblPhoneTo.Name = "lblPhoneTo";
            this.lblPhoneTo.Size = new System.Drawing.Size(106, 29);
            this.lblPhoneTo.TabIndex = 1;
            this.lblPhoneTo.Text = "Destino:";
            this.lblPhoneTo.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tbPhoneTo
            // 
            this.tbPhoneTo.Location = new System.Drawing.Point(127, 49);
            this.tbPhoneTo.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tbPhoneTo.Name = "tbPhoneTo";
            this.tbPhoneTo.Size = new System.Drawing.Size(383, 29);
            this.tbPhoneTo.TabIndex = 1;
            this.tbPhoneTo.Text = "+54911";
            // 
            // lblBroker
            // 
            this.lblBroker.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBroker.Location = new System.Drawing.Point(13, 87);
            this.lblBroker.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblBroker.Name = "lblBroker";
            this.lblBroker.Size = new System.Drawing.Size(106, 29);
            this.lblBroker.TabIndex = 3;
            this.lblBroker.Text = "Proveedor:";
            this.lblBroker.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // Tabs
            // 
            this.Tabs.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Tabs.Controls.Add(this.tabPage1);
            this.Tabs.Controls.Add(this.tabPage2);
            this.Tabs.Controls.Add(this.tabPage3);
            this.Tabs.Controls.Add(this.tabPage4);
            this.Tabs.Controls.Add(this.tabPage5);
            this.Tabs.Controls.Add(this.tabPage6);
            this.Tabs.Location = new System.Drawing.Point(13, 197);
            this.Tabs.Name = "Tabs";
            this.Tabs.SelectedIndex = 0;
            this.Tabs.Size = new System.Drawing.Size(721, 176);
            this.Tabs.TabIndex = 4;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.tbMessageText);
            this.tabPage1.Location = new System.Drawing.Point(4, 30);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(713, 142);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Texto";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tbMessageText
            // 
            this.tbMessageText.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbMessageText.Location = new System.Drawing.Point(6, 7);
            this.tbMessageText.Multiline = true;
            this.tbMessageText.Name = "tbMessageText";
            this.tbMessageText.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbMessageText.Size = new System.Drawing.Size(701, 129);
            this.tbMessageText.TabIndex = 0;
            // 
            // tabPage2
            // 
            this.tabPage2.Location = new System.Drawing.Point(4, 30);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(581, 165);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Imagen";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // tabPage3
            // 
            this.tabPage3.Location = new System.Drawing.Point(4, 30);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(581, 165);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Audio";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // tabPage4
            // 
            this.tabPage4.Location = new System.Drawing.Point(4, 30);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Size = new System.Drawing.Size(581, 165);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "Video";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // tabPage5
            // 
            this.tabPage5.Location = new System.Drawing.Point(4, 30);
            this.tabPage5.Name = "tabPage5";
            this.tabPage5.Size = new System.Drawing.Size(581, 165);
            this.tabPage5.TabIndex = 4;
            this.tabPage5.Text = "Contacto";
            this.tabPage5.UseVisualStyleBackColor = true;
            // 
            // tabPage6
            // 
            this.tabPage6.Location = new System.Drawing.Point(4, 30);
            this.tabPage6.Name = "tabPage6";
            this.tabPage6.Size = new System.Drawing.Size(581, 165);
            this.tabPage6.TabIndex = 5;
            this.tabPage6.Text = "Sticker";
            this.tabPage6.UseVisualStyleBackColor = true;
            // 
            // btSend
            // 
            this.btSend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btSend.BackColor = System.Drawing.SystemColors.Control;
            this.btSend.Enabled = false;
            this.btSend.Location = new System.Drawing.Point(566, 49);
            this.btSend.Name = "btSend";
            this.btSend.Size = new System.Drawing.Size(164, 68);
            this.btSend.TabIndex = 5;
            this.btSend.Text = "Enviar Mensaje";
            this.btSend.UseVisualStyleBackColor = false;
            // 
            // lblWebhook
            // 
            this.lblWebhook.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblWebhook.Location = new System.Drawing.Point(13, 127);
            this.lblWebhook.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblWebhook.Name = "lblWebhook";
            this.lblWebhook.Size = new System.Drawing.Size(106, 29);
            this.lblWebhook.TabIndex = 6;
            this.lblWebhook.Text = "Webhook:";
            this.lblWebhook.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cbWebhook
            // 
            this.cbWebhook.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cbWebhook.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.cbWebhook.FormattingEnabled = true;
            this.cbWebhook.Location = new System.Drawing.Point(127, 128);
            this.cbWebhook.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cbWebhook.Name = "cbWebhook";
            this.cbWebhook.Size = new System.Drawing.Size(383, 29);
            this.cbWebhook.TabIndex = 3;
            this.cbWebhook.Text = "http://localhost.fiddler:51560/Events/Inbound";
            // 
            // lblPhoneFrom
            // 
            this.lblPhoneFrom.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPhoneFrom.Location = new System.Drawing.Point(13, 9);
            this.lblPhoneFrom.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblPhoneFrom.Name = "lblPhoneFrom";
            this.lblPhoneFrom.Size = new System.Drawing.Size(106, 29);
            this.lblPhoneFrom.TabIndex = 7;
            this.lblPhoneFrom.Text = "Remitente:";
            this.lblPhoneFrom.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cbPhoneFrom
            // 
            this.cbPhoneFrom.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cbPhoneFrom.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.cbPhoneFrom.FormattingEnabled = true;
            this.cbPhoneFrom.Location = new System.Drawing.Point(127, 10);
            this.cbPhoneFrom.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cbPhoneFrom.Name = "cbPhoneFrom";
            this.cbPhoneFrom.Size = new System.Drawing.Size(383, 29);
            this.cbPhoneFrom.TabIndex = 0;
            this.cbPhoneFrom.Text = "+54911";
            // 
            // ssStatus
            // 
            this.ssStatus.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsLbStatus});
            this.ssStatus.Location = new System.Drawing.Point(0, 386);
            this.ssStatus.Name = "ssStatus";
            this.ssStatus.Size = new System.Drawing.Size(746, 22);
            this.ssStatus.SizingGrip = false;
            this.ssStatus.TabIndex = 8;
            // 
            // tsLbStatus
            // 
            this.tsLbStatus.Name = "tsLbStatus";
            this.tsLbStatus.Size = new System.Drawing.Size(16, 17);
            this.tsLbStatus.Text = "...";
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(746, 408);
            this.Controls.Add(this.ssStatus);
            this.Controls.Add(this.cbPhoneFrom);
            this.Controls.Add(this.lblPhoneFrom);
            this.Controls.Add(this.lblWebhook);
            this.Controls.Add(this.btSend);
            this.Controls.Add(this.Tabs);
            this.Controls.Add(this.lblBroker);
            this.Controls.Add(this.tbPhoneTo);
            this.Controls.Add(this.lblPhoneTo);
            this.Controls.Add(this.cbWebhook);
            this.Controls.Add(this.cbBroker);
            this.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.Name = "Main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Whatsapp Webhook - Tester";
            this.Tabs.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.ssStatus.ResumeLayout(false);
            this.ssStatus.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cbBroker;
        private System.Windows.Forms.Label lblPhoneTo;
        private System.Windows.Forms.TextBox tbPhoneTo;
        private System.Windows.Forms.Label lblBroker;
        private System.Windows.Forms.TabControl Tabs;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.TabPage tabPage5;
        private System.Windows.Forms.TabPage tabPage6;
        private System.Windows.Forms.Button btSend;
        private System.Windows.Forms.Label lblWebhook;
        private System.Windows.Forms.ComboBox cbWebhook;
        private System.Windows.Forms.TextBox tbMessageText;
        private System.Windows.Forms.Label lblPhoneFrom;
        private System.Windows.Forms.ComboBox cbPhoneFrom;
        private System.Windows.Forms.StatusStrip ssStatus;
        private System.Windows.Forms.ToolStripStatusLabel tsLbStatus;
    }
}

