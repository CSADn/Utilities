using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Web.Script.Serialization;

namespace Brightness
{
    public partial class Form1 : Form
    {
        private readonly string _version = "1.7.7";
        private bool _vis = false;
        private MenuItem _mi_updt;
        private bool _updateChecked = false;
        private readonly Color _backColor = Color.FromArgb(31, 31, 31);
        private readonly Color _textColor = Color.White;
        private DateTime _deactivateTime;

        public Form1()
        {
            InitializeComponent();
        }

        public bool CheckForUpdate_ShowAtCtxMenu()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            var client = new WebClient();
            client.Headers.Add("User-Agent", "Win10_BrightnessSlider_cfu_" + Environment.MachineName);

            var serializer = new JavaScriptSerializer();
            var response = client.DownloadString("https://api.github.com/repos/blackholeearth/Win10_BrightnessSlider/releases/latest");
            var json = serializer.Deserialize<dynamic>(response);
            var latest_version = json["tag_name"].Replace("v", string.Empty);
            var releases_html_url = "https://github.com/blackholeearth/Win10_BrightnessSlider/releases";
            var newVersionFound = Version.Parse(latest_version) > Version.Parse(this._version);

            base.Invoke((MethodInvoker) delegate {
                this._mi_updt.Enabled = true;
                if (!newVersionFound)
                {
                    this._mi_updt.Text = "UpToDate";
                    this._mi_updt.Click += (sd, ev) => MessageBox.Show("Already Up to Date!!!.");
                }
                else
                {
                    this._mi_updt.Text = "***New Version Found: v" + latest_version;
                    this._mi_updt.Click += (sd, ev) => this.MessageTextbox_Show("download Latest From Here:\r\n" + releases_html_url);
                }
            });
            return true;
        }

        private void CreateNotifyIcon_ContexMenu()
        {
            var menu = new ContextMenu();

            var item = new MenuItem("Exit", (snd, ev) => Application.Exit());

            var item2 = new MenuItem("About Me - (v" + this._version + ")", delegate (object snd, EventArgs ev) {
                string text = "\r\nDeveloper: blackholeearth \r\n\r\nOfficial Site: \r\nhttps://github.com/blackholeearth/Win10_BrightnessSlider\r\n";
                this.MessageTextbox_Show(text);
            });

            this._mi_updt = new MenuItem("Will Check For Update...(in 60sec)", delegate (object snd, EventArgs ev) {});
            this._mi_updt.Enabled = false;

            Task.Factory.StartNew(delegate {
                int num = 1;
                while (true)
                {
                    if (num >= 5)
                    {
                        break;
                    }
                    Thread.Sleep(0x1388);
                    if (!Debugger.IsAttached)
                    {
                        Thread.Sleep((int) (0xd6d8 * num));
                    }
                    try
                    {
                        this._updateChecked = this.CheckForUpdate_ShowAtCtxMenu();
                    }
                    catch (Exception exception)
                    {
                        FileLogger.Log("@update-check: " + exception, true, "CreateNotifyIcon_ContexMenu", @"D:\e_Belgelerim\Coding_Projects\vs2017\Win10_BrightnessSlider\Win10_BrightnessSlider\Form1.cs", 0x134);
                    }
                    if (this._updateChecked)
                    {
                        break;
                    }
                    num++;
                }
            });

            var item3 = new MenuItem("Detect Monitors", (snd, ev) => this.RePopulateSliders());
            var item4 = new MenuItem("Run At Startup", delegate (object snd, EventArgs ev) {
                var mi = (snd as MenuItem);
                mi.Checked = !item.Checked;
                HelperFn.SetStartup(mi.Checked);
            });

            menu.MenuItems.Add(item);
            menu.MenuItems.Add(item2);
            menu.MenuItems.Add(this._mi_updt);
            menu.MenuItems.Add(item3);
            menu.MenuItems.Add(item4);

            if (Debugger.IsAttached)
            {
                var item5 = new MenuItem("State Of Window", delegate (object snd, EventArgs ev) {
                    var textArray1 = new string[] { "visible:", base.Visible.ToString(), "\r\nFocused:", this.Focused.ToString(), "\r\ncanFocus:", base.CanFocus.ToString(), "\r\n" };
                    MessageBox.Show(string.Concat(textArray1));
                });

                var item6 = new MenuItem("All Screens Info", (snd, ev) => MessageBox.Show("------------GetAllMonitorInfo----------" + string.Join("--------\r\n", HelperFn.GetAllMonitorInfo().ToArray()) + "\r\n\r\n------------GetAllScreensInfo----------" + string.Join("--------\r\n", HelperFn.GetAllScreensInfo().ToArray())));

                menu.MenuItems.Add(new MenuItem("-"));
                menu.MenuItems.Add(item5);
                menu.MenuItems.Add(item6);
            }
            this.notifyIcon1.ContextMenu = menu;
        }

        public void eSetVis(bool visible)
        {
            Console.WriteLine("eSetVis - vis:" + this._vis.ToString());

            base.WindowState = FormWindowState.Normal;
            base.StartPosition = FormStartPosition.Manual;

            var point = HelperFn.CalculateFormLocationNearToTaskbarDatePart(base.Size);
            var point2 = new Point(-base.Width, -base.Height);

            if (!visible)
            {
                base.TopMost = false;
                base.Hide();
                this._vis = false;
            }
            else
            {
                base.Location = point;
                base.TopMost = true;
                base.BringToFront();
                base.Show();
                base.Activate();
                this._vis = true;

                if (this.fLayPnl1.Controls.Count > 0)
                    this.fLayPnl1.Controls[0].Focus();
            }
        }

        public void FixFormHeight()
        {
            List<uc_brSlider> list = Enumerable.ToList<uc_brSlider>(Enumerable.OfType<uc_brSlider>(this.fLayPnl1.Controls));
            try
            {
                base.Height = Enumerable.Count<uc_brSlider>(list) * list[0].Height;
            }
            catch (Exception)
            {
                base.Height = 100;
            }
        }

        private void Form1_Deactivate(object sender, EventArgs e)
        {
            this._deactivateTime = DateTime.Now;
            this.eSetVis(false);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.notifyIcon1.Text = "Brightness";
            base.FormBorderStyle = FormBorderStyle.None;
            base.WindowState = FormWindowState.Normal;
            base.ShowInTaskbar = false;
            base.StartPosition = FormStartPosition.Manual;
            base.Location = new Point(-base.Width, -base.Height);
            this.BackColor = this._backColor;
            this.notifyIcon1.MouseClick += new MouseEventHandler(this.NotifyIcon1_MouseClick);
            base.Deactivate += new EventHandler(this.Form1_Deactivate);
            this.CreateNotifyIcon_ContexMenu();
            this.UpdateStatesOnGuiControls();
            this.eSetVis(false);
            this.RePopulateSliders();
        }

        private void Form1_MouseEnter(object sender, EventArgs e)
        {
            this.eSetVis(true);
            HelperFn.ColorFormIfActive_inDbgMode(this);
            if (Debugger.IsAttached)
            {
                Console.WriteLine("Active Window ProcName:" + HelperFn.GetActiveProcessFileName());
            }
        }

        private void MessageTextbox_Show(string text)
        {
            Form form1 = new Form();
            form1.Width = 0x278;
            form1.Height = 200;
            form1.Padding = new Padding(7);
            form1.StartPosition = FormStartPosition.CenterScreen;
            Form form = form1;
            TextBox box1 = new TextBox();
            box1.ReadOnly = true;
            box1.Dock = DockStyle.Fill;
            box1.Multiline = true;
            TextBox box = box1;
            box.Font = new Font(box.Font.OriginalFontName, 12f);
            form.Controls.Add(box);
            form.Show();
            box.Text = text;
        }

        private void NotifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if (DateTime.Now.Subtract(this._deactivateTime).TotalMilliseconds >= 300.0)
            {
                this.notifyIcon1.MouseClick -= new MouseEventHandler(this.NotifyIcon1_MouseClick);
                base.Deactivate -= new EventHandler(this.Form1_Deactivate);
                if (e.Button == MouseButtons.Left)
                {
                    this.UpdateAllSliderControls();
                    this.eSetVis(!this._vis);
                    HelperFn.ColorFormIfActive_inDbgMode(this);
                }
                this.notifyIcon1.MouseClick += new MouseEventHandler(this.NotifyIcon1_MouseClick);
                base.Deactivate += new EventHandler(this.Form1_Deactivate);
            }
        }

        private void RePopulateSliders()
        {
            this.fLayPnl1.Controls.Clear();

            var list = RichInfoScreen.Get_RichInfo_Screen();

            if (list == null)
                MessageBox.Show("SORRY!!\r\ndxva2 and WMI functions Failed\r\n There is Nothing to do");
            else
            {
                foreach (RichInfoScreen screen in list)
                {
                    var slider = new uc_brSlider
                    {
                        Margin = Padding.Empty,
                        BackColor = this._backColor,
                        riScreen = screen
                    };

                    slider.label1.ForeColor = this._textColor;

                    HelperFn.SetTooltip(slider.pictureBox1, screen.TooltipText, screen.TooltipTitle);

                    this.fLayPnl1.Controls.Add(slider);
                }
            }
            this.FixFormHeight();
        }

        public void UpdateAllSliderControls()
        {
            foreach (uc_brSlider slider in Enumerable.ToList<uc_brSlider>(Enumerable.OfType<uc_brSlider>(this.fLayPnl1.Controls)))
            {
                slider.UpdateSliderControl();
            }
            this.FixFormHeight();
        }

        public void UpdateNotifyIconText()
        {
            var sb = new StringBuilder();
            var sliders = fLayPnl1.Controls.OfType<uc_brSlider>();

            if (sliders.Count() > 1)
            {
                var n = 1;
                foreach (var slider in sliders)
                    sb.AppendLine($"Pantalla Nº{n++}: {slider.trackBar1.Value}");
            }
            else
            {
                sb.Append($"{sliders.First().trackBar1.Value}%");
            }

            this.notifyIcon1.Text = sb.ToString();
        }

        private void UpdateStatesOnGuiControls()
        {
            bool flag = HelperFn.isRunAtStartup();
            Enumerable.FirstOrDefault<MenuItem>((IEnumerable<MenuItem>) (from x in Enumerable.Cast<MenuItem>(this.notifyIcon1.ContextMenu.MenuItems)
                where x.Text == "Run At Startup"
                select x)).Checked = flag;
            this.UpdateAllSliderControls();
        }

        protected override System.Windows.Forms.CreateParams CreateParams
        {
            get
            {
                System.Windows.Forms.CreateParams createParams = base.CreateParams;
                createParams.ExStyle |= 0x80;
                return createParams;
            }
        }
    }
}

