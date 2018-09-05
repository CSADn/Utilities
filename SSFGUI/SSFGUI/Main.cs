using PeanutButter.TrayIcon;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace SSFGUI
{
    //
    // https://securesocketfunneling.github.io/ssf/#how-to-use-socks
    //
    public partial class Main : Form
    {
        private TrayIcon _ti;
        private IPAddress _clientIP;
        private Process _client;
        private Process _server;
        private Mode _running;
        private string _socketport;
        private string _serverport;
        private System.Timers.Timer _verify;

        private enum Mode
        {
            Off,
            Client,
            Server
        }


        public Main()
        {
            InitializeComponent();

            _running = Mode.Off;

            Load += Main_Load;
            FormClosing += Main_FormClosing;
            Resize += Main_Resize;

            _ti = new TrayIcon(Properties.Resources.TrayIcon);
            _ti.NotifyIcon.Text = "SSF - GUI";
            _ti.NotifyIcon.MouseDoubleClick += NotifyIcon_MouseDoubleClick;

            btClient.Click += BtClient_Click;
            btClient.MouseHover += BtClient_MouseHover;

            btServer.Click += BtServer_Click;
            btServer.MouseHover += BtServer_MouseHover;

            cbVerify.CheckedChanged += CbVerify_CheckedChanged;
            cbVerify.MouseHover += CbVerify_MouseHover;

            tbIPAddress.MouseHover += TbIPAddress_MouseHover;

            tbIPAddress.Text = ConfigurationManager.AppSettings["client_ip"];

            _socketport = ConfigurationManager.AppSettings["socket_port"];
            _serverport = ConfigurationManager.AppSettings["server_port"];

            _verify = new System.Timers.Timer();
            _verify.Enabled = false;
            _verify.Interval = 5000;
            _verify.Elapsed += (s, e) => verifyConnection();
        }


        private void Main_Load(object sender, EventArgs e)
        {
            _ti.Show();
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            switch (_running)
            {
                case Mode.Client:
                    StopClient();
                    break;

                case Mode.Server:
                    _verify.Enabled = false;
                    _verify.Stop();
                    StopServer();
                    break;
            }

            _ti.Hide();
            _ti.Dispose();
        }

        private void Main_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
                Hide();
        }


        private void NotifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (WindowState != FormWindowState.Normal && e.Button == MouseButtons.Left)
            {
                Show();
                WindowState = FormWindowState.Normal;
                Focus();
            }
        }


        private void BtClient_Click(object sender, EventArgs e)
        {
            switch (_running)
            {
                case Mode.Off:
                    StartClient();
                    break;

                case Mode.Client:
                    StopClient();
                    break;
            }
        }

        private void BtServer_Click(object sender, EventArgs e)
        {
            switch (_running)
            {
                case Mode.Off:
                    StartServer();
                    break;

                case Mode.Server:
                    _verify.Enabled = false;
                    _verify.Stop();
                    StopServer();
                    break;
            }
        }

        private void BtServer_MouseHover(object sender, EventArgs e)
        {
            tooltip.ToolTipTitle = "Servidor";
            tooltip.SetToolTip(btServer, "Segundo paso.\r\nDebe iniciarse en la computadora\r\nque va a compartir recursos.");
        }

        private void BtClient_MouseHover(object sender, EventArgs e)
        {
            tooltip.ToolTipTitle = "Cliente";
            tooltip.SetToolTip(btClient, "Primer paso.\r\nDebe iniciarse en la computadora\r\nque accede a los recursos compartidos.");
        }

        private void CbVerify_CheckedChanged(object sender, EventArgs e)
        {
            var cb = (CheckBox)sender;

            if (_running == Mode.Server)
            {
                _verify.Enabled = cb.Checked;

                if ((_server == null || _server.HasExited))
                    StopServer();
            }

            Debug.Print($"Recursivo: {(cb.Checked ? "SI" : "NO")}");
        }

        private void CbVerify_MouseHover(object sender, EventArgs e)
        {
            tooltip.ToolTipTitle = "Conexión recursiva";
            tooltip.SetToolTip(cbVerify, "Intentar conectarse al cliente cada 5 segundos.\r\n(La conexión es invertida)");
        }

        private void TbIPAddress_MouseHover(object sender, EventArgs e)
        {
            tooltip.ToolTipTitle = "IP Cliente";
            tooltip.SetToolTip(tbIPAddress, "IP de la computadora cliente utilizada en modo servidor.\r\n(La conexión es invertida)");
        }


        private void StartClient()
        {
            _running = Mode.Client;

            _client = Process.Start(new ProcessStartInfo
            {
                FileName = "ssfd.exe",
                Arguments = $"-g -p {_serverport}",
                WindowStyle = (cbDebug.Checked ? ProcessWindowStyle.Normal : ProcessWindowStyle.Hidden)
            });

            this.

            _client.EnableRaisingEvents = true;
            _client.Exited += (s, e) =>
            {
                try { this.Invoke(StopClient); }
                catch { }
            };

            UpdateInterface();
        }

        private void StopClient()
        {
            if (_client != null && !_client.HasExited)
                _client.Kill();

            _running = Mode.Off;

            UpdateInterface();
        }

        private void StartServer()
        {
            if (!IPAddress.TryParse(tbIPAddress.Text, out _clientIP))
            {
                MessageBox.Show("Dirección IP inválida", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                tbIPAddress.SelectAll();
                tbIPAddress.Focus();
                return;
            }

            _running = Mode.Server;

            LaunchSSF();

            _server.EnableRaisingEvents = true;
            _server.Exited += (s, e) =>
            {
                try { this.Invoke(StopServer); }
                catch { }
            };

            _verify.Enabled = cbVerify.Checked;

            UpdateInterface();
        }

        private void StopServer()
        {
            if (_server != null && !_server.HasExited)
                _server.Kill();

            _running = Mode.Off;
            _verify.Enabled = false;
            _verify.Stop();

            UpdateInterface();
        }


        private void LaunchSSF()
        {
            _server = Process.Start(new ProcessStartInfo
            {
                FileName = "ssf.exe",
                Arguments = $"-g -F 0.0.0.0:{_socketport} -p {_serverport} {_clientIP}",
                WindowStyle = (cbDebug.Checked ? ProcessWindowStyle.Normal : ProcessWindowStyle.Hidden)
            });
        }

        private void UpdateInterface()
        {
            switch (_running)
            {
                case Mode.Off:
                    Text = "SSF - GUI";
                    btClient.Text = "#1 Cliente";
                    btClient.Enabled = true;
                    btServer.Text = "#2 Servidor";
                    btServer.Enabled = true;
                    tbIPAddress.Enabled = true;
                    tbIPAddress.SelectAll();
                    tbIPAddress.Focus();
                    cbVerify.Enabled = true;
                    cbDebug.Enabled = true;
                    _ti.NotifyIcon.Text = "SSF - GUI";
                    Show();
                    WindowState = FormWindowState.Normal;
                    break;

                case Mode.Client:
                    Text = "SSF - GUI [Modo Cliente!]";
                    btClient.Text = "Finalizar Cliente";
                    btClient.Enabled = true;
                    btServer.Text = "#2 Servidor";
                    btServer.Enabled = false;
                    tbIPAddress.Enabled = false;
                    cbVerify.Enabled = false;
                    cbDebug.Enabled = false;
                    _ti.NotifyIcon.Text = "SSF - GUI [Modo Cliente!]";
                    WindowState = FormWindowState.Minimized;
                    break;

                case Mode.Server:
                    Text = "SSF - GUI [Modo Servidor!]";
                    btClient.Text = "#1 Cliente";
                    btClient.Enabled = false;
                    btServer.Text = "Finalizar Servidor";
                    btServer.Enabled = true;
                    tbIPAddress.Enabled = false;
                    cbVerify.Enabled = true;
                    cbDebug.Enabled = false;
                    _ti.NotifyIcon.Text = "SSF - GUI [Modo Servidor!]";
                    WindowState = FormWindowState.Minimized;
                    break;
            }
        }


        private void verifyConnection()
        {
            var serverNotRunning = (_server == null || _server.HasExited);

            if (serverNotRunning)
                LaunchSSF();

            Debug.Print($"Tick: {(serverNotRunning ? "NO" : "SI")}");
        }
    }
}
