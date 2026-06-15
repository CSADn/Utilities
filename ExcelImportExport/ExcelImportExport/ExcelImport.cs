using Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace ExcelImportExport
{
    public partial class ExcelImport : Form
    {
        public ExcelImport()
        {
            InitializeComponent();
            AddEvents();
        }
        
        private void AddEvents()
        {
            ExitBtn.Click += ExitBtn_Click;
            OpenBtn.Click += OpenBtn_Click;
            FormClosed += ExcelImport_FormClosed;
        }

        private void ExcelImport_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void OpenBtn_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            System.IO.Stream streamExcel = null;

            ofd.InitialDirectory = Application.ExecutablePath;
            ofd.Filter = "Archivos Excel (*.xls;*.xlsx)|*.xls;*.xlsx";
            ofd.FilterIndex = 1;
            if(ofd.ShowDialog(this) == DialogResult.OK)
            {
                try
                {
                    if((streamExcel = ofd.OpenFile()) != null)
                    {
                        FileTxt.Text = ofd.FileName;
                        ProcessExcel(streamExcel);
                    }
                }
                catch (Exception ex)
                {
                    StatusLbl.Text = "Error al procesar"; 
                    MessageBox.Show(this, ex.InnerException?.Message??ex.Message);
                }
            }

        }

        private void ExitBtn_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void ProcessExcel(Stream stream)
        {
            ExcelWrapper ewrapper = new ExcelWrapper(stream);

            List<Clases.InterfaceTest> lista = ewrapper.GetListFromSheet<Clases.InterfaceTest>();

            if(lista != null && lista.Count > 0)
                StatusLbl.Text = $"Leídos {lista.Count}";
            else
                StatusLbl.Text = $"Sin Registros";
        }
    }
}
