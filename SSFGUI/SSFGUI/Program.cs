using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace SSFGUI
{
    static class Program
    {
        private const int WM_GETTEXT = 0xD;
        private const int SW_RESTORE = 9;

        public static bool InDebug;

        [STAThread]
        static void Main(string[] args)
        {
            if (IsAlreadyRunning())
            {
                BringToFront();
                return;
            }

            InDebug = args != null && args.Length > 0 && args.First().Equals("-debug", StringComparison.InvariantCultureIgnoreCase);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Main());
        }

        private static bool IsAlreadyRunning()
        {
            var created = false;

            var mutex = new Mutex(true, "Global\\SSFGUIAPP", out created);

            if (created)
                mutex.ReleaseMutex();

            return !created;
        }

        private static void BringToFront()
        {
            var buffer = new StringBuilder(256);
            var h = IntPtr.Zero;

            do
            {
                h = FindWindowEx(IntPtr.Zero, h, null, null);
                SendMessage(h, WM_GETTEXT, 256, buffer);

                if (buffer.ToString().StartsWith("SSF - GUI"))
                {
                    ShowWindow(h, SW_RESTORE);
                    SetForegroundWindow(h);
                    break;
                }
            }
            while (h != IntPtr.Zero);
        }


        [DllImport("User32.dll")]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string strClassName, string strWindowName);

        [System.Runtime.InteropServices.DllImport("User32.dll", EntryPoint = "SendMessage", CharSet = System.Runtime.InteropServices.CharSet.Auto)] //
        public static extern bool SendMessage(IntPtr hWnd, uint uMsg, int wParam, StringBuilder lParam);

        [DllImport("User32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("User32.dll")]
        private static extern int ShowWindow(IntPtr hwnd, int cmd);
    }
}
