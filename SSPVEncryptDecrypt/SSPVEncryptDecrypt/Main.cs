using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Helpers;

namespace SSPVEncryptDecrypt
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
        }

        private void Encrypt_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SaltEncrypt.Text)) //para solo encriptar con el salt por defecto
                TargetEncrypt.Text = Utilities.Encrypt(SourceEncrypt.Text);
            else
                TargetEncrypt.Text = Utilities.Encrypt(SourceEncrypt.Text, SaltEncrypt.Text);
        }

        private void Decrypt_Click(object sender, EventArgs e)
        {
            TargetDecrypt.Text = Utilities.Decrypt(SourceDecrypt.Text, SaltDecrypt.Text);
        }
    }
}
