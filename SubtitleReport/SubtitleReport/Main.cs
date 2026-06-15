using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SubtitleReport
{
    public partial class Main : Form
    {
        private List<DataGridViewColumn> _dgColumns;


        public Main()
        {
            InitializeComponent();

            BindControlEvents();
            SetupGrid();
        }


        private void BindControlEvents()
        {
            btBrowse.Click += BtBrowse_Click;
            btReload.Click += BtReload_Click;
        }

        private void BtBrowse_Click(object sender, EventArgs e)
        {
            var o = new FolderBrowserDialog();

            o.ShowNewFolderButton = false;

            if (o.ShowDialog() == DialogResult.OK)
            {
                tbPath.Text = o.SelectedPath;
                Analyze();
            }
            else
                tbPath.Text = string.Empty;
        }

        private void BtReload_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbPath.Text))
                return;

            Analyze();
        }


        private void SetupGrid()
        {
            dgResults.ReadOnly = true;
            dgResults.AutoGenerateColumns = false;
            dgResults.RowHeadersVisible = true;
            dgResults.AllowUserToResizeRows = false;
            dgResults.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgResults.Font = new Font("sans-serif", 7, FontStyle.Regular);

            _dgColumns = new List<DataGridViewColumn>();
            _dgColumns.AddColumn("File", width: 0);
            _dgColumns.AddColumn("Folder", width: 0);
            _dgColumns.AddColumn("Size", width: -1, align: DataGridViewContentAlignment.MiddleRight);
            _dgColumns.AddColumn("Bytes", visible: false);

            dgResults.Columns.AddRange(_dgColumns.ToArray());

            dgResults.RowPostPaint += DgResults_RowPostPaint;
            dgResults.CellMouseDoubleClick += DgResults_CellMouseDoubleClick;
        }

        private void DgResults_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            var grid = (sender as DataGridView);

            if (grid.SelectedRows.Count == 0)
                return;

            var data = (Source)grid.SelectedRows[0].DataBoundItem;
            var args = $"/select,\"{Path.Combine(data.Folder, data.File)}\"";

            System.Diagnostics.Process.Start("explorer.exe", args);
        }

        private void DgResults_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            var grid = (sender as DataGridView);
            var rowIdx = (e.RowIndex + 1).ToString();

            var centerFormat = new StringFormat()
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            var textSize = TextRenderer.MeasureText(rowIdx, Font);

            if (grid.RowHeadersWidth < textSize.Width + 5)
                grid.RowHeadersWidth = textSize.Width + 5;

            var headerBounds = new Rectangle(e.RowBounds.Left, e.RowBounds.Top, grid.RowHeadersWidth, e.RowBounds.Height);

            e.Graphics.DrawString(rowIdx, Font, SystemBrushes.ControlText, headerBounds, centerFormat);
        }


        private List<string> Process(string dir)
        {
            var dirs = Directory.GetDirectories(dir);
            var c = 1;

            var ext = new List<string> { ".mp4", ".avi", ".mkv" };
            var files = new List<string>();

            this.Invoke(() =>
            {
                tsLabel.Text = string.Empty;
                tsProgress.Value = 0;
                tsProgress.Maximum = dirs.Count();
                tsProgress.Visible = true;
            });

            foreach (var d in dirs)
            {
                this.Invoke(() =>
                {
                    tsProgress.Value = c++;
                    tsLabel.Text = $"Directorios... ({c}/{dirs.Count()})";
                });

                var f = Directory
                    .GetFiles(d, "*.*", SearchOption.TopDirectoryOnly)
                    .Where(w =>
                        ext.Any(a =>
                            a.Equals(Path.GetExtension(w).ToLower())
                        )
                    );

                if (f.Count() > 0)
                    files.AddRange(f);
            }

            this.Invoke(() =>
            {
                tsLabel.Text = string.Empty;
                tsProgress.Value = 0;
                tsProgress.Maximum = files.Count();
            });

            var output = new List<string>();
            var filter = 3;
            // 1 = No SRT
            // 2 = No SRT Exact Name
            // 3 = No SRT + "es"

            c = 1;

            foreach (var f in files)
            {
                this.Invoke(() =>
                {
                    tsProgress.Value = c++;
                    tsLabel.Text = $"Archivos... ({c}/{files.Count()})";
                });

                var p = Path.GetDirectoryName(f);
                var n = Path.GetFileNameWithoutExtension(f).ToLower();

                if (n.EndsWith(".es"))
                    continue;

                var srt = Directory.GetFiles(p, "*.srt", SearchOption.TopDirectoryOnly);

                if (srt.Count() == 0)
                {
                    output.Add(f);
                    continue;
                }

                switch (filter)
                {
                    case 2:
                        if (!srt.Any(a => Path.GetFileNameWithoutExtension(a).ToLower().Equals(n)))
                        {
                            output.Add(f);
                            continue;
                        }
                        break;

                    case 3:
                        if (!srt.Any(a => Path.GetFileName(a).ToLower().EndsWith(".es.srt")))
                        {
                            output.Add(f);
                            continue;
                        }
                        break;
                }
            }

            this.Invoke(() =>
            {
                tsLabel.Text = string.Empty;
                tsProgress.Value = 0;
                tsProgress.Maximum = 100;
                tsProgress.Visible = false;
            });

            return output;
        }

        private string FileSize(string fullPath)
        {
            var length = new FileInfo(fullPath).Length;
            var suf = new string[] { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB

            if (length == 0)
                return $"0{suf[0]}";

            var abs = Math.Abs(length);
            var place = Convert.ToInt32(Math.Floor(Math.Log(abs, 1024)));
            var num = Math.Round(abs / Math.Pow(1024, place), 1);

            return $"{Math.Sign(length) * num} {suf[place]}";
        }

        private void Analyze()
        {
            btBrowse.Enabled = false;
            btReload.Enabled = false;
            dgResults.DataSource = null;

            var results = new SortableBindingList<Source>();
            results.SwitchPropertySort("Size", "Bytes");

            Task.Factory.StartNew(() =>
            {
            var missings = Process(tbPath.Text);

            foreach (var m in missings)
                results.Add(new Source
                {
                    Folder = Path.GetDirectoryName(m),
                    File = Path.GetFileName(m),
                    Size = FileSize(m),
                    Bytes = new FileInfo(m).Length
                });

                this.Invoke(() =>
                {
                    dgResults.DataSource = results;
                    dgResults.Sort(dgResults.Columns.GetFirstColumn(DataGridViewElementStates.None), ListSortDirection.Ascending);

                    btBrowse.Enabled = true;
                    btReload.Enabled = true;
                });
            });

        }


    }
}
