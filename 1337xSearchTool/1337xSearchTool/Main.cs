using ADn.WebHelper;
using ADn.WebHelper.Extensions;
using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Taskbar;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace _1337xSearchTool
{
    public partial class Main : CustomForm
    {
        #region Globals

        private List<Torrent> _source;
        private List<Torrent> _featured;
        private List<Torrent> _torrents;
        private List<string> _pages;
        private Dictionary<string, string> _categories;
        private KeyValuePair<string, string> _selectedCat;
        private int _torrentsPerPage;
        private int _torrentsToGet;

        private List<DataGridViewTextBoxColumn> _gridColumns;

        private readonly IConfiguration _configuration;

        private readonly string _urlBase;
        private readonly string _urlHome;
        private readonly string _urlPage;
        private readonly string _urlDownload;
        private readonly string _urlTorrent;

        private const string _tokenId = "[@Id]";
        private const string _tokenCat = "[@Cat]";
        private const string _tokenPage = "[@Page]";
        private const string _tokenUrlName = "[@UrlName]";

        private const string _torCache = "https://torcache.net/";
        private const string _torCacheSSL = "https://torcache.net/";

        private int _sortColumnIndex;
        private SortOrder _sortOrder;

        private DateTime _lastUpdate;

        private const string _sourceFilename = "Source-[@Cat].xml";
        private const string _registryFilterText = @"Software\ADn\1337xSearchTool\FilterText";

        private Color _color1 = Color.FromArgb(100, 230, 159, 31);
        private Color _color2 = Color.Green;
        private Color _color3 = Color.Red;
        private Color _color4 = Color.FromArgb(100, 79, 107, 114);

        private BackgroundWorker _bworker;
        private System.Threading.Timer _topMost;

        private TaskbarManager _taskbar;

        private readonly string _userAgent;
        private List<Cookie> _cookies;

        #endregion

        #region Constructor

        public Main()
        {
            InitializeComponent();

            var configFileName = "appsettings.json";

            if (!File.Exists(configFileName))
                throw new Exception("Configuration file not found.");

            _configuration = new ConfigurationBuilder()
                .AddJsonFile(configFileName, optional: false, reloadOnChange: false)
                .Build();

            _urlBase = _configuration.GetValue<string>("urlBase");
            _urlHome = $"{_urlBase}{_configuration.GetValue<string>("urlHome")}";
            _urlPage = $"{_urlBase}{_configuration.GetValue<string>("urlPage")}";
            _urlDownload = $"{_urlBase}{_configuration.GetValue<string>("urlDownload")}";
            _urlTorrent = $"{_urlBase}{_configuration.GetValue<string>("urlTorrent")}";

            _userAgent = _configuration.GetValue<string>("userAgent");

            var cookies = _configuration.GetSection("cookies").Get<Dictionary<string, string>>();
            _cookies = ParseCookies(cookies);

            _featured = new List<Torrent>();
            _torrents = new List<Torrent>();
            _pages = new List<string>();
            _torrentsPerPage = 0;

            _torrentsToGet = 1000;
            tbItemsToGetValue.Text = _torrentsToGet.ToString();

            _sortColumnIndex = 0;
            _sortOrder = SortOrder.Descending;

            _taskbar = TaskbarManager.Instance;
            _topMost = new System.Threading.Timer(
                callback: (state) => this.Invoke((MethodInvoker)(() =>
                {
                    this.TopMost = false;
                    Win32.StealFocus(this);
                })),
                state: null,
                dueTime: Timeout.Infinite,
                period: Timeout.Infinite
            );
        }

        #endregion

        #region Controls Events

        private void Main_Load(object sender, EventArgs e)
        {
            SetStatus("Loading...");

            SetupGrid();
            SetupFilter();

            GetCategories();
            GetPopularTorrents();

            ProcessResize();
        }

        private void Main_Resize(object sender, EventArgs e)
        {
            ProcessResize();
        }

        private void Main_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Escape)
                return;

            if (chFilter.Checked)
                chFilter.Checked = false;
        }

        private void Main_Shown(object sender, EventArgs e)
        {
            tbFilter.Focus();
        }

        private void btDownload_Click(object sender, EventArgs e)
        {
            DownloadTorrent();
        }

        private void btGet_Click(object sender, EventArgs e)
        {
            GetTorrentsPages();
        }

        private void chFilter_CheckedChanged(object sender, EventArgs e)
        {
            FilterGrid(chFilter.Checked);
        }

        private void btGoto_Click(object sender, EventArgs e)
        {
            GotoTorrentPage();
        }

        private void btSave_Click(object sender, EventArgs e)
        {
            SaveSource();
        }

        private void btLoad_Click(object sender, EventArgs e)
        {
            LoadSource();
        }

        private void tbFilter_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
                return;

            chFilter.Checked = true;
            e.Handled = true;
        }

        private void tbItemsToGetValue_TextChanged(object sender, EventArgs e)
        {
            int value;
            if (!int.TryParse(tbItemsToGetValue.Text, out value))
            {
                tbItemsToGetValue.Text = "";
                return;
            }

            _torrentsToGet = value;
        }

        private void dgGrid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgGrid.Rows.Count == 1)
                return;

            if (e.ColumnIndex == dgGrid.Columns["Size"].Index)
            {
                e.Value = GetDisplaySize((long)e.Value);
                e.FormattingApplied = true;
            }
        }

        private void dgGrid_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            _sortOrder = ((_sortOrder == SortOrder.Descending) && (_sortColumnIndex == e.ColumnIndex) ? SortOrder.Ascending : SortOrder.Descending);
            SortGrid(e.ColumnIndex);
        }

        private void dgGrid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            Torrent t = (Torrent)dgGrid.CurrentRow.DataBoundItem;

            if (t != null)
            {
                if (t.InPage >= 0)
                    lbInPageValue.Text = t.InPage.ToString();
                else if (t.InPage == -1)
                    lbInPageValue.Text = "Featured";
                else
                    lbInPageValue.Text = "...";
            }
        }

        private void dgGrid_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            GotoTorrentPage();
        }

        private void dgGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
                GotoTorrentPage();
        }

        private void cbColumns_SelectedIndexChanged(object sender, EventArgs e)
        {
            PopulateFilterOperators();
        }

        private void cbCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            _selectedCat = (KeyValuePair<string, string>)cbCategory.SelectedItem;
        }

        #endregion

        #region Private Methods

        private void ProcessResize()
        {
            tsPBar.Width = this.Width - 288;

            if (dgGrid.Rows.Count > 0)
            {
                int autoSizeCol = 2;

                int colswidth = _gridColumns
                    .Where(w => w.Index != autoSizeCol)
                    .Sum(s => s.Width);

                dgGrid.Columns[autoSizeCol].Width = dgGrid.Width - colswidth - 25;
            }
        }

        private void SetupGrid()
        {
            dgGrid.ReadOnly = true;
            dgGrid.AutoGenerateColumns = false;
            dgGrid.RowHeadersVisible = false;
            dgGrid.AllowUserToResizeRows = false;
            dgGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgGrid.Font = new Font("sans-serif", 7, FontStyle.Regular);

            _gridColumns = new List<DataGridViewTextBoxColumn>();
            DataGridViewTextBoxColumn column;

            column = new DataGridViewTextBoxColumn();
            column.Name = "Id";
            column.HeaderText = "Id";
            column.DataPropertyName = "Id";
            column.ValueType = typeof(long);
            column.Width = 50;
            column.SortMode = DataGridViewColumnSortMode.Automatic;

            _gridColumns.Add(column);

            column = new DataGridViewTextBoxColumn();
            column.Name = "Type";
            column.HeaderText = "Type";
            column.DataPropertyName = "Type";
            column.ValueType = typeof(string);
            column.Width = 80;
            column.DefaultCellStyle.ForeColor = _color1;
            column.DefaultCellStyle.Font = new Font(dgGrid.Font, FontStyle.Bold);
            column.SortMode = DataGridViewColumnSortMode.Automatic;

            _gridColumns.Add(column);

            column = new DataGridViewTextBoxColumn();
            column.Name = "Name";
            column.HeaderText = "Name";
            column.DataPropertyName = "Name";
            column.ValueType = typeof(string);
            column.Width = 600;
            column.DefaultCellStyle.ForeColor = _color1;
            column.DefaultCellStyle.Font = new Font(dgGrid.Font, FontStyle.Bold);
            column.SortMode = DataGridViewColumnSortMode.Automatic;

            _gridColumns.Add(column);

            column = new DataGridViewTextBoxColumn();
            column.Name = "Seeders";
            column.HeaderText = "SE";
            column.DataPropertyName = "Seeders";
            column.ValueType = typeof(int);
            column.Width = 50;
            column.DefaultCellStyle.ForeColor = _color2;
            column.DefaultCellStyle.Font = new Font(dgGrid.Font, FontStyle.Bold);
            column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            column.SortMode = DataGridViewColumnSortMode.Automatic;

            _gridColumns.Add(column);

            column = new DataGridViewTextBoxColumn();
            column.Name = "Leechers";
            column.HeaderText = "LE";
            column.DataPropertyName = "Leechers";
            column.ValueType = typeof(int);
            column.Width = 50;
            column.DefaultCellStyle.ForeColor = _color3;
            column.DefaultCellStyle.Font = new Font(dgGrid.Font, FontStyle.Bold);
            column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            column.SortMode = DataGridViewColumnSortMode.Automatic;

            _gridColumns.Add(column);

            column = new DataGridViewTextBoxColumn();
            column.Name = "Size";
            column.HeaderText = "Size";
            column.DataPropertyName = "Size";
            column.ValueType = typeof(long);
            column.Width = 80;
            column.DefaultCellStyle.ForeColor = _color4;
            column.DefaultCellStyle.Font = new Font(dgGrid.Font, FontStyle.Bold);
            column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            column.SortMode = DataGridViewColumnSortMode.Automatic;

            _gridColumns.Add(column);

            column = new DataGridViewTextBoxColumn();
            column.Name = "Uploader";
            column.HeaderText = "Uploader";
            column.DataPropertyName = "Uploader";
            column.ValueType = typeof(string);
            column.SortMode = DataGridViewColumnSortMode.Automatic;

            _gridColumns.Add(column);

            dgGrid.Columns.AddRange(_gridColumns.ToArray());
        }

        private void SetupFilter()
        {
            PopulateFilterColumns();
            InitializeFilterText();
        }

        private void PopulateFilterColumns()
        {
            cbColumns.Items.Clear();

            Dictionary<int, string> list = new Dictionary<int, string>();

            foreach (DataGridViewTextBoxColumn c in _gridColumns)
                list.Add(c.Index, c.Name);

            cbColumns.DataSource = new BindingSource(list, null);
            cbColumns.DisplayMember = "Value";
            cbColumns.ValueMember = "Key";

            var name = list.FirstOrDefault(f => f.Value.Equals("Name"));

            if (name.Value != null)
                cbColumns.SelectedItem = name;
        }

        private void PopulateGrid()
        {
            dgGrid.DataSource = _source;
            lbItemsShownValue.Text = _source.Count.ToString();
        }

        private void InitializeFilterText()
        {
            tbFilter.Text = Registry.CurrentUser
                .GetValue(_registryFilterText, string.Empty)
                .ToString();
        }

        private void PersistFilterText()
        {
            Registry.CurrentUser.SetValue(_registryFilterText, tbFilter.Text.Trim());
        }

        private void GetCategories()
        {
            SetStatus("Retrieving Categories...");
            tsPBar.Style = ProgressBarStyle.Marquee;

            var li = WebHelper
                .RetrieveElementsByTag("main", _urlHome, _cookies, _userAgent)
                .ElementsByClass("list-box")
                .Take(1)
                .ToList()
                .ElementsByTag("ul")
                .ElementsByTag("li");

            if (li.Count == 0)
                throw new Exception("No existe la lista de categorías");

            _categories = new Dictionary<string, string>();

            _categories = li
                .Where(w =>
                {
                    var a = WebHelper
                        .ElementsByTag("a", w)
                        .FirstOrDefault();

                    return (
                        a != null &&
                        a.Attributes.Count > 0 &&
                        a.Attributes["href"].Value.StartsWith("/cat/")
                    );
                })
                .Select(s =>
                    WebHelper
                        .ElementsByTag("a", s)
                        .FirstOrDefault()
                )
                .ToDictionary(
                    k => Regex.Match(k.Attributes["href"].Value, @"\/cat\/(.*)\/\d\/").Groups[1].Value,
                    v => v.InnerText
                );

            cbCategory.DataSource = new BindingSource(_categories, null);
            cbCategory.DisplayMember = "Value";
            cbCategory.ValueMember = "Key";

            var movies = _categories.SingleOrDefault(s => s.Value.Contains("Movies"));

            if (movies.Value != null)
                cbCategory.SelectedItem = movies;

            tsPBar.Style = ProgressBarStyle.Continuous;
            SetStatus("Done.!.");
        }

        private void GetPopularTorrents()
        {
            _bworker = new BackgroundWorker();
            _bworker.WorkerReportsProgress = true;

            _bworker.DoWork += new DoWorkEventHandler(GetPopularTorrentsAsync);
            _bworker.ProgressChanged += new ProgressChangedEventHandler(_bworker_ProgressChanged);
            _bworker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(GetPopularTorrentsAsyncCompleted);

            _bworker.RunWorkerAsync();
        }

        private void GetPopularTorrentsAsync(object sender, DoWorkEventArgs e)
        {
            BWReportMarquee("Retrieving Popular Torrents...");

            var document = WebHelper.RetrieveWebPageDocument(_urlHome, _cookies, _userAgent, Encoding.UTF8);
            var featured = WebHelper
                .ElementsByClass("featured-list", document)
                .Where(node =>
                    WebHelper.ElementsByTagAndProperty("strong", "InnerText", "movie", node).Count > 0
                )
                .ToList()
                .ElementsByTag("tbody")
                .ElementsByTag("tr");

            if (featured == null)
                throw new HtmlWebException("'featured-box' class not found.");

            if (featured.Count == 0)
                throw new Exception("Featured movie torrents no existe");

            Application.DoEvents();

            foreach (var tr in featured)
            {
                BWReportProgress("Parsing Torrents List...", featured.Count);

                Application.DoEvents();

                var torrent = ParseRow(tr);

                _featured.Add(torrent);
            }

            _source = _featured;
        }

        private void ParseIdUrlName(HtmlNode div, out int id, out string urlname)
        {
            throw new NotImplementedException();
        }

        private void GetPopularTorrentsAsyncCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            UpdateLastDate(DateTime.Now);

            PopulateGrid();
            SortGrid();

            SetStatus("Done.!.");

            GetInitialParameters();
        }

        private void GetInitialParameters()
        {
            _bworker = new BackgroundWorker();
            _bworker.WorkerReportsProgress = true;

            _bworker.DoWork += new DoWorkEventHandler(GetInitialParametersAsync);
            _bworker.ProgressChanged += new ProgressChangedEventHandler(_bworker_ProgressChanged);
            _bworker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(GetInitialParametersAsyncCompleted);

            _bworker.RunWorkerAsync();
        }

        private void GetInitialParametersAsync(object sender, DoWorkEventArgs e)
        {
            BWReportMarquee("Retrieving First Page...");

            var url = _urlPage
                .Replace(_tokenCat, _selectedCat.Key)
                .Replace(_tokenPage, "1");

            var firstPageSource = WebHelper.RetrieveWebPageSource(url, _cookies, _userAgent);

            if (string.IsNullOrEmpty(firstPageSource))
                throw new Exception("Cannot retrieve URL");

            _pages.Add(firstPageSource);

            var pageTorrents = GetTorrentsFromPage(0);

            if (pageTorrents.Count > 0)
            {
                _torrents.AddRange(pageTorrents);
                _torrentsPerPage = pageTorrents.Count;
            }
        }

        private void GetInitialParametersAsyncCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            lbPageItemsValue.Text = _torrentsPerPage.ToString();
            SetStatus("Done.!.");
        }

        private List<Torrent> GetTorrentsFromPage(int pageidx)
        {
            var list = new List<Torrent>();

            var document = WebHelper.WebSourceToHtml(_pages[pageidx]);

            var torrents = WebHelper
                .ElementsByClass("featured-list", document)
                .ElementsByTag("tbody")
                .ElementsByTag("tr");

            if (torrents.Count == 0)
                throw new Exception("No hay torrents");

            int lastId = _torrentsToGet - _torrents.Count();

            foreach (var tr in torrents)
            {
                lastId--;

                var torrent = ParseRow(tr);

                list.Add(torrent);
            }

            return list;
        }

        private void GetTorrentsPages()
        {
            _bworker = new BackgroundWorker();
            _bworker.WorkerReportsProgress = true;

            _bworker.DoWork += new DoWorkEventHandler(GetTorrentsPagesAsync);
            _bworker.ProgressChanged += new ProgressChangedEventHandler(_bworker_ProgressChanged);
            _bworker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(GetTorrentsPagesAsyncCompleted);

            _bworker.RunWorkerAsync();
        }

        private void GetTorrentsPagesAsync(object sender, DoWorkEventArgs e)
        {
            BWReportMarquee("Preparing to bulk retrieve...");

            Application.DoEvents();
            Thread.Sleep(1000);

            var url = _urlPage.Replace(_tokenCat, _selectedCat.Key);

            var pagesCount = (_torrentsToGet / _torrentsPerPage);

            _pages.Clear();
            _torrents.Clear();

            for (int i = 1; i <= pagesCount; i++)
            {
                BWReportProgress("Retrieving page " + (i + 1) + "/" + pagesCount, pagesCount);

                Application.DoEvents();

                try
                {
                    var pageSource = WebHelper.RetrieveWebPageSource(url.Replace(_tokenPage, (i + 1).ToString()), _cookies, _userAgent);

                    if (!string.IsNullOrEmpty(pageSource))
                        _pages.Add(pageSource);
                }
                catch
                {
                    MessageBox.Show("Se produjo un error al intentar descargar la página: '" + (i + 1) + "'");
                }
            }

            for (int i = 0; i < _pages.Count; i++)
            {
                BWReportProgress("Processing page " + (i + 1) + "/" + pagesCount, pagesCount);

                Application.DoEvents();

                try
                {
                    var pageTorrents = GetTorrentsFromPage(i);

                    if (pageTorrents.Count > 0)
                        _torrents.AddRange(pageTorrents);
                }
                catch
                {
                    MessageBox.Show("Se produjo un error al intentar procesar la página: '" + (i + 1) + "'");
                }
            }

            if (_torrents.Count > 0)
                _source = _torrents;
        }

        private void GetTorrentsPagesAsyncCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            UpdateLastDate(DateTime.Now);

            PopulateGrid();
            SortGrid();

            SetStatus("Done.!.");
        }

        private void ParseTypeIdUrlName(HtmlNode node, out string type, out int id, out string url, out string name)
        {
            var col1 = WebHelper.ElementsByClass("coll-1", node).FirstOrDefault();
            var strong = WebHelper.ElementsByTag("strong", col1).FirstOrDefault();
            var title = WebHelper.ElementsByTag("a", strong).FirstOrDefault();
            string[] href = title.Attributes["href"].Value.Split('/');

            id = Convert.ToInt32(href[2]);
            url = href[3];
            name = title.InnerText;
            type = string.Empty;
        }

        private void ParseUploaderNick(HtmlNode node, out string nick)
        {
            var col5 = WebHelper.ElementsByClass("coll-5", node).FirstOrDefault();
            nick = col5.InnerText;
        }

        private Torrent ParseRow(HtmlNode node)
        {
            var tr = new List<HtmlNode> { node };

            var name = tr.ElementsByClass("name");

            var id = name
                .ElementsByTag("a")
                .ElementAt(1)
                .Attributes["href"]
                    .Value
                    .Split('/')[2];

            var type = name
                .ElementsByTag("a")
                .ElementsByTag("i")
                .First()
                .Attributes["class"]
                    .Value
                    .Replace("flaticon-", string.Empty);

            var title = name
                .ElementsByTag("a")
                .ElementAt(1)
                .InnerText;

            var url = name
                .ElementsByTag("a")
                .ElementAt(1)
                .Attributes["href"]
                    .Value
                    .Split('/')[3];

            var seeds = tr.ElementsByClass("seeds").FirstOrDefault();
            var leeches = tr.ElementsByClass("leeches").FirstOrDefault();
            var size = tr.ElementsByClass("size").FirstOrDefault();
            var nick = tr.ElementsByTag("td").Last();

            return new Torrent
            {
                Id = Convert.ToInt32(id),
                Type = type,
                Name = title,
                UrlName = url,
                Seeders = Convert.ToInt32(seeds.InnerText),
                Leechers = Convert.ToInt32(leeches.InnerText),
                Size = ParseSize(size.FirstChild.InnerText),
                Uploader = nick.InnerText,
                InPage = -1
            };
        }

        private long ParseSize(string size)
        {
            var retval = 0L;
            var dc = NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;

            var unit = size.Substring(size.Length - 2, 2);
            var number = decimal.Parse(size.Replace(unit, string.Empty), new CultureInfo("en-US"));

            switch (unit.ToLower())
            {
                case "kb":
                    retval = Convert.ToInt64(number * 1024);
                    break;
                case "mb":
                    retval = Convert.ToInt64(number * 1048576);
                    break;
                case "gb":
                    retval = Convert.ToInt64(number * 1073741824);
                    break;
                default:
                    retval = Convert.ToInt64(number);
                    break;
            }

            return retval;
        }

        private int ParseHealth(HtmlNode node)
        {
            var retval = 0;

            var div = WebHelper.ElementsByClass("health", node).FirstOrDefault();

            if (div != null)
            {
                var health = div.Attributes["style"]
                    .Value
                    .Replace("width: ", "")
                    .Replace("%;", "");

                retval = Convert.ToInt32(health);
            }

            return retval;
        }

        private string GetDisplaySize(long size)
        {
            var retval = "";

            var unit = new string[] { "XX", "KB", "MB", "GB" };
            var unitidx = 0;

            var value = Convert.ToDecimal(size);

            while (Math.Round(value) >= 1024)
            {
                value = value / 1024;
                unitidx++;
            }

            retval = value.ToString("#.00") + unit[unitidx];

            return retval;
        }

        private string GetDisplayHealth(int health)
        {
            return health.ToString() + "%";
        }

        private void DownloadTorrent()
        {
            var t = (Torrent)dgGrid.CurrentRow.DataBoundItem;
            //DownloadTorrent(t);
            DownloadMagnet(t);
        }

        private void DownloadMagnet(Torrent t)
        {
            btDownload.Enabled = false;

            var document = WebHelper.RetrieveWebPageDocument(
                _urlTorrent
                    .Replace(_tokenId, t.Id.ToString())
                    .Replace(_tokenUrlName, t.UrlName),
                _cookies,
                _userAgent,
                Encoding.UTF8
            );

            var details = WebHelper.ElementsByClass("category-detail", document).FirstOrDefault();

            if (details == null)
            {
                btDownload.Enabled = true;
                return;
            }

            var linksContainer = WebHelper.ElementsByClass("download-links", details).FirstOrDefault();

            if (linksContainer == null)
            {
                btDownload.Enabled = true;
                return;
            }

            var links = WebHelper.ElementsByTag("li", linksContainer);

            var magnet = links[0].Descendants("a").FirstOrDefault().Attributes["href"].Value;

            try
            {
                Process.Start(magnet);
            }
            catch { }

            WindowState = FormWindowState.Minimized;

            btDownload.Enabled = true;
        }

        private void DownloadTorrent(Torrent t)
        {
            try
            {
                var document = WebHelper.RetrieveWebPageDocument(
                    _urlTorrent
                        .Replace(_tokenId, t.Id.ToString())
                        .Replace(_tokenUrlName, t.UrlName),
                    _cookies,
                    _userAgent,
                    Encoding.UTF8
                );

                var details = WebHelper.ElementsByClass("category-detail", document).FirstOrDefault();

                if (details == null)
                    return;

                var linksContainer = WebHelper.ElementsByClass("download-links", details).FirstOrDefault();

                if (linksContainer == null)
                    return;

                var links = WebHelper.ElementsByTag("li", linksContainer);

                var link = links[1].Descendants("a").FirstOrDefault().Attributes["href"].Value;

                if (link.StartsWith(_torCache))
                    link = link.Replace(_torCache, _torCacheSSL);

                var filename = string.Empty;
                var file = WebHelper.DownloadFile(link, out filename);

                var f = new SaveFileDialog()
                {
                    Title = "Save Torrent..",
                    Filter = "Torrents|*.torrent",
                    OverwritePrompt = true,
                    FileName = filename
                };

                if (f.ShowDialog() == DialogResult.OK)
                {
                    using (BinaryWriter bw = new BinaryWriter(File.Create(f.FileName)))
                    {
                        bw.Write(file);
                        bw.Flush();
                        bw.Close();
                    }
                }

                file = null;
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show("File not found", "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SortGrid()
        {
            SortGrid(_sortColumnIndex);
        }

        private void SortGrid(int columnIndex)
        {
            var c = dgGrid.Columns[columnIndex];

            var direction = (_sortOrder == SortOrder.Ascending) ? " ASC" : " DESC";

            var listSource = (List<Torrent>)dgGrid.DataSource;

            listSource = listSource
                .OrderBy(c.Name + direction)
                .ToList();

            dgGrid.DataSource = listSource;

            c.HeaderCell.SortGlyphDirection = _sortOrder;
            _sortColumnIndex = columnIndex;
        }

        private void UpdateProgress(int value, int max)
        {
            if (tsPBar.Minimum != 0)
                tsPBar.Minimum = 0;

            if (tsPBar.Maximum != max)
                tsPBar.Maximum = max;

            if (tsPBar.Value != value)
                tsPBar.Value = value;
        }

        private void GotoTorrentPage()
        {
            var t = (Torrent)dgGrid.CurrentRow.DataBoundItem;

            if (t == null)
                return;

            var url = _urlTorrent
                .Replace(_tokenId, t.Id.ToString())
                .Replace(_tokenUrlName, t.UrlName);

            this.TopMost = true;
            Process.Start(url);
            _topMost.Change(300, Timeout.Infinite);
        }

        private void PopulateFilterOperators()
        {
            var c = (KeyValuePair<int, string>)cbColumns.SelectedItem;
            var dc = dgGrid.Columns[c.Key];
            var list = new Dictionary<Operator, string>();

            if (dc.ValueType == typeof(string))
            {
                list.Add(Operator.Complex, Operator.Complex.ToString());
                list.Add(Operator.Contains, Operator.Contains.ToString());
                list.Add(Operator.NotContains, Operator.NotContains.ToString());
                list.Add(Operator.StartsWith, Operator.StartsWith.ToString());
                list.Add(Operator.EndsWith, Operator.EndsWith.ToString());
            }
            else if (dc.ValueType == typeof(Int64) || dc.ValueType == typeof(Int32))
            {
                list.Add(Operator.Greater, Operator.Greater.ToString());
                list.Add(Operator.GreaterEqual, Operator.GreaterEqual.ToString());
                list.Add(Operator.Lower, Operator.Lower.ToString());
                list.Add(Operator.LowerEqual, Operator.LowerEqual.ToString());
            }

            list.Add(Operator.Equal, Operator.Equal.ToString());
            list.Add(Operator.Distinct, Operator.Distinct.ToString());

            cbOperators.DataSource = new BindingSource(list, null);
            cbOperators.DisplayMember = "Value";
            cbOperators.ValueMember = "Key";
        }

        private void FilterGrid(bool applied)
        {
            if (string.IsNullOrEmpty(tbFilter.Text))
                return;

            FilterReadOnly(applied);
            PersistFilterText();

            if (!applied)
            {
                PopulateGrid();
                SortGrid();
                return;
            }

            var c = (KeyValuePair<int, string>)cbColumns.SelectedItem;
            var o = (KeyValuePair<Operator, string>)cbOperators.SelectedItem;

            var listSource = (List<Torrent>)dgGrid.DataSource;

            var prop = typeof(Torrent).GetProperty(c.Value);

            var ds = NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;

            var s = tbFilter.Text.ToLower();
            var d = 0.0M;

            switch (o.Key)
            {
                case Operator.Contains:
                    var list = s
                        .Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                        .ToList();

                    listSource = listSource
                        .Where(w => list.All(a => (prop.GetValue(w, null) as string).ToLower().Contains(a)))
                        .ToList();
                    break;
                case Operator.NotContains:
                    listSource = listSource
                        .Where(w => !(prop.GetValue(w, null) as string).ToLower().Contains(s))
                        .ToList();
                    break;
                case Operator.Lower:
                    d = Convert.ToDecimal(s.Replace(",", ".").Replace(".", ds));
                    listSource = listSource
                        .Where(w => Convert.ToDecimal(prop.GetValue(w, null)) < d)
                        .ToList();
                    break;
                case Operator.LowerEqual:
                    d = Convert.ToDecimal(s.Replace(",", ".").Replace(".", ds));
                    listSource = listSource
                        .Where(w => Convert.ToDecimal(prop.GetValue(w, null)) <= d)
                        .ToList();
                    break;
                case Operator.Greater:
                    d = Convert.ToDecimal(s.Replace(",", ".").Replace(".", ds));
                    listSource = listSource
                        .Where(w => Convert.ToDecimal(prop.GetValue(w, null)) > d)
                        .ToList();
                    break;
                case Operator.GreaterEqual:
                    d = Convert.ToDecimal(s.Replace(",", ".").Replace(".", ds));
                    listSource = listSource
                        .Where(w => Convert.ToDecimal(prop.GetValue(w, null)) >= d)
                        .ToList();
                    break;
                case Operator.Equal:
                    listSource = listSource
                        .Where(w => prop.GetValue(w, null).ToString().ToLower().Equals(s))
                        .ToList();
                    break;
                case Operator.Distinct:
                    listSource = listSource
                        .Where(w => prop.GetValue(w, null).ToString().ToLower() != s)
                        .ToList();
                    break;
                case Operator.StartsWith:
                    listSource = listSource
                        .Where(w => (prop.GetValue(w, null) as string).ToLower().StartsWith(s))
                        .ToList();
                    break;
                case Operator.EndsWith:
                    listSource = listSource
                        .Where(w => (prop.GetValue(w, null) as string).ToLower().EndsWith(s))
                        .ToList();
                    break;
                case Operator.Complex:
                    listSource = listSource
                        .Where(FilterGridWhereBuilder(s, prop))
                        .ToList();
                    break;

                default:
                    break;
            }

            dgGrid.DataSource = listSource;
            lbItemsShownValue.Text = listSource.Count.ToString();
            SortGrid();
        }

        private Func<Torrent, bool> FilterGridWhereBuilder(string filters, PropertyInfo pi)
        {
            if (string.IsNullOrWhiteSpace(filters))
                return new Func<Torrent, bool>(w => true);

            var ands = filters.ToLower().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            return new Func<Torrent, bool>(w =>
            {
                var assert = true;
                var value = pi
                    .GetValue(w, null)
                    .ToString()
                    .ToLower();

                foreach (var a in ands)
                {
                    var ors = a.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                    var inc = ors.Where(ow => !ow.StartsWith("!"));
                    var exc = ors.Where(ow => ow.StartsWith("!")).Select(s => s.Substring(1));

                    if (inc.Any())
                        assert &= inc.Any(ow => value.Contains(ow));

                    if (exc.Any())
                        assert &= !exc.Any(ow => value.Contains(ow));
                }

                return assert;
            });
        }

        private void FilterReadOnly(bool applied)
        {
            cbColumns.Enabled = !applied;
            cbOperators.Enabled = !applied;
            tbFilter.Enabled = !applied;

            if (!applied)
            {
                tbFilter.Focus();
                tbFilter.SelectAll();
            }
        }

        private void SaveSource()
        {
            var path = Path.GetDirectoryName(Application.ExecutablePath) + "\\" + _sourceFilename.Replace(_tokenCat, _selectedCat.Value);
            Utilities.SaveSource(_source, path, _lastUpdate);

            MessageBox.Show("done!", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void LoadSource()
        {
            var path = Path.GetDirectoryName(Application.ExecutablePath) + "\\" + _sourceFilename.Replace(_tokenCat, _selectedCat.Value);

            try
            {
                _source = Utilities.LoadSource(path, out _lastUpdate);
                UpdateLastDate(_lastUpdate);

                PopulateGrid();
                SortGrid();

                ResetFilter();

                MessageBox.Show("done!", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show(_selectedCat.Value + " doesn't downloaded yet.", "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void UpdateLastDate(DateTime date)
        {
            _lastUpdate = date;
            lbLastUpdateValue.Text = _lastUpdate.ToString("dd/MM/yyyy HH:mm:ss");
        }

        private void ResetFilter()
        {
            cbColumns.SelectedIndex = 0;
            cbOperators.SelectedIndex = 0;
            tbFilter.Text = "";

            chFilter.Checked = false;

            FilterReadOnly(false);
        }

        private List<Cookie> ParseCookies(Dictionary<string, string> cookies)
        {
            var domain = new UriBuilder(_urlBase).Host.Replace("www", "");

            return cookies
                .Select(s => new Cookie(s.Key, s.Value, "/", domain.ToString()))
                .ToList();
        }

        #endregion

        #region Status Bar

        public void SetStatus(string text)
        {
            SetStatus(text, false);
        }

        public void SetStatus(string text, bool showbar)
        {
            if (string.IsNullOrEmpty(text))
                tsStatus.Visible = false;

            tsStatus.Text = text;
            tsStatus.Visible = true;
            tsPBar.Visible = showbar;

            try
            {
                _taskbar.SetProgressState(TaskbarProgressBarState.NoProgress);
            }
            catch { }
        }

        public void SetStatus(int min, int max)
        {
            tsPBar.Value = min;
            tsPBar.Minimum = min;
            tsPBar.Maximum = max;
        }

        #endregion

        #region BackgroundWorker

        private void _bworker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            var p = (BWProgress)e.UserState;

            tsStatus.Text = p.Message;

            if (p.Marquee)
            {
                tsPBar.Style = ProgressBarStyle.Marquee;
                _taskbar.SetProgressState(TaskbarProgressBarState.Indeterminate);
            }
            else if (tsPBar.Style != ProgressBarStyle.Continuous)
            {
                tsPBar.Style = ProgressBarStyle.Continuous;
                _taskbar.SetProgressState(TaskbarProgressBarState.Normal);
            }

            if (tsPBar.Value == p.Max)
            {
                tsPBar.Value = p.Min;
                _taskbar.SetProgressState(TaskbarProgressBarState.NoProgress);
            }

            tsPBar.Minimum = p.Min;
            tsPBar.Maximum = p.Max;

            if (p.Marquee)
                tsPBar.Value = p.Min;
            else
            {
                tsPBar.PerformStep();
                _taskbar.SetProgressValue(tsPBar.Value, p.Max);
            }

            if (!tsPBar.Visible)
                tsPBar.Visible = true;

            Application.DoEvents();
        }

        private void BWReportMarquee(string message)
        {
            _bworker.ReportProgress(0, new BWProgress()
            {
                Message = message,
                Marquee = true,
                Min = 0,
                Max = 100
            });
        }

        private void BWReportProgress(string message, int max)
        {
            BWReportProgress(message, 0, max);
        }

        private void BWReportProgress(string message, int min, int max)
        {
            _bworker.ReportProgress(0, new BWProgress()
            {
                Min = min,
                Max = max,
                Message = message
            });
        }

        #endregion
    }
}
