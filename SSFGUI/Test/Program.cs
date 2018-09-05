using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Test
{
    class Program
    {
        private const int WM_GETTEXT = 0xD;

        static void Main(string[] args)
        {
            var buffer = new StringBuilder(256);
            var h = IntPtr.Zero;

            do
            {
                h = FindWindowEx(IntPtr.Zero, h, null, null);
                //GetWindowText(h, out buffer, 256);
                SendMessage(h, WM_GETTEXT, 256, buffer);

                if (buffer.ToString().StartsWith("SSF - GUI"))
                {
                    Console.WriteLine($"Title: {buffer}");
                    //break;
                }
            }
            while (h != IntPtr.Zero);

            Console.ReadKey();
        }

        [DllImport("User32.dll")]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string strClassName, string strWindowName);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetWindowText(IntPtr hWnd, out StringBuilder lpString, int nMaxCount);

        [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = System.Runtime.InteropServices.CharSet.Auto)] //
        public static extern bool SendMessage(IntPtr hWnd, uint uMsg, int wParam, StringBuilder lParam);
    }
}
