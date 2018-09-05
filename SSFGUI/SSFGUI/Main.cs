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
    public partial class Main : Form
    {
        private TrayIcon _ti;
        private IPAddress _serverIP;
        private Process _client;
        private Process _server;
        private Process _delegate;
        private Mode _running;
        private ProcessWindowStyle _wstyle;
        private string _ssfcport;
        private string _ssfsport;
        private string _delegateport;
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
            _wstyle = Program.InDebug ? ProcessWindowStyle.Normal : ProcessWindowStyle.Hidden;

            Load += Main_Load;
            FormClosing += Main_FormClosing;
            Resize += Main_Resize;

            _ti = new TrayIcon(Properties.Resources.TrayIcon);
            _ti.NotifyIcon.Text = "SSF - GUI";
            _ti.NotifyIcon.MouseDoubleClick += NotifyIcon_MouseDoubleClick;

            btClient.Click += BtClient_Click;
            btServer.Click += BtServer_Click;
            cbVerify.CheckedChanged += CbVerify_CheckedChanged;

            _ssfcport = ConfigurationManager.AppSettings["ssfc_port"];
            _ssfsport = ConfigurationManager.AppSettings["ssfs_port"];
            _delegateport = ConfigurationManager.AppSettings["delegate_port"];

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


        private void StartClient()
        {
            _running = Mode.Client;

            _client = Process.Start(new ProcessStartInfo
            {
                FileName = "ssfs.exe",
                Arguments = $"-p {_ssfsport}",
                WindowStyle = _wstyle
            });

            this.

            _client.EnableRaisingEvents = true;
            _client.Exited += (s, e) => this.Invoke(StopClient);

            _delegate = Process.Start(new ProcessStartInfo
            {
                FileName = "delegate.exe",
                Arguments = $"-P{_delegateport} SERVER=http SOCKS=127.0.0.1:{_ssfcport} RELIABLE=* ADMIN=email@email.com",
                WindowStyle = _wstyle
            });

            _delegate.EnableRaisingEvents = true;
            _delegate.Exited += (s, e) => this.Invoke(StopClient);

            UpdateInterface();
        }

        private void StopClient()
        {
            if (_client != null && !_client.HasExited)
                _client.Kill();

            if (_delegate != null && !_delegate.HasExited)
                _delegate.Kill();

            _running = Mode.Off;

            UpdateInterface();
        }

        private void StartServer()
        {
            if (!IPAddress.TryParse(tbIPAddress.Text, out _serverIP))
            {
                MessageBox.Show("Invalid IP Address", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                tbIPAddress.SelectAll();
                tbIPAddress.Focus();
                return;
            }

            _running = Mode.Server;

            LaunchSSFC();

            _server.EnableRaisingEvents = true;
            _server.Exited += (s, e) => this.Invoke(StopServer);

            _verify.Enabled = cbVerify.Checked;

            UpdateInterface();
        }

        private void StopServer()
        {
            if (cbVerify.Checked)
                return;

            if (_server != null && !_server.HasExited)
                _server.Kill();

            _running = Mode.Off;
            _verify.Enabled = false;
            _verify.Stop();

            UpdateInterface();
        }


        private void LaunchSSFC()
        {
            _server = Process.Start(new ProcessStartInfo
            {
                FileName = "ssfc.exe",
                Arguments = $"-F {_ssfcport} -p{_ssfsport} {_serverIP}",
                WindowStyle = _wstyle
            });
        }

        private void UpdateInterface()
        {
            switch (_running)
            {
                case Mode.Off:
                    Text = "SSF - GUI";
                    btClient.Text = "Client";
                    btClient.Enabled = true;
                    btServer.Text = "Server";
                    btServer.Enabled = true;
                    tbIPAddress.Enabled = true;
                    tbIPAddress.SelectAll();
                    tbIPAddress.Focus();
                    _ti.NotifyIcon.Text = "SSF - GUI";
                    Show();
                    WindowState = FormWindowState.Normal;
                    break;

                case Mode.Client:
                    Text = "SSF - GUI [Client running!]";
                    btClient.Text = "Kill Client";
                    btClient.Enabled = true;
                    btServer.Text = "Server";
                    btServer.Enabled = false;
                    tbIPAddress.Enabled = false;
                    _ti.NotifyIcon.Text = "SSF - GUI [Client running!]";
                    WindowState = FormWindowState.Minimized;
                    break;

                case Mode.Server:
                    Text = "SSF - GUI [Server running!]";
                    btClient.Text = "Client";
                    btClient.Enabled = false;
                    btServer.Text = "Kill Server";
                    btServer.Enabled = true;
                    tbIPAddress.Enabled = false;
                    _ti.NotifyIcon.Text = "SSF - GUI [Server running!]";
                    WindowState = FormWindowState.Minimized;
                    break;
            }
        }


        private void verifyConnection()
        {
            var serverNotRunning = (_server == null || _server.HasExited);

            if (serverNotRunning)
                LaunchSSFC();

            Debug.Print($"Tick: {(serverNotRunning ? "NO" : "SI")}");
        }
    }
}
