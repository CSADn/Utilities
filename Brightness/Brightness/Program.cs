using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Brightness
{
    internal static class Program
    {
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;
        private static bool _winKey = false;
        private static Form1 _form;
        private static Timer _shown;

        private static void Main()
        {
            if (Environment.OSVersion.Version.Major >= 6)
                SetProcessDPIAware();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            _hookID = SetHook(_proc);

            _shown = new Timer();
            _shown.Interval = 1000;
            _shown.Tick += (s, e) =>
            {
                _shown.Stop();
                _form.eSetVis(false);
            };
            _shown.Enabled = false;

            _form = new Form1();

            Application.Run(_form);

            UnhookWindowsHookEx(_hookID);
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                if (wParam == (IntPtr)WM_KEYDOWN)
                {
                    var vkCode = Marshal.ReadInt32(lParam);
                    var key = (Keys)vkCode;

                    if (key == Keys.RWin || key == Keys.LWin)
                    {
                        if (!_winKey)
                        {
                            _winKey = true;
                            Debug.WriteLine($"WinKey! Down!");
                        }
                    }
                    else if (key == Keys.PageUp)
                    {
                        if (_winKey)
                        {
                            Debug.WriteLine($"[WIN] + [PGUP]");
                            _form.UpdateAllSliderControls();
                            _form.eSetVis(true);
                            _shown.Stop();
                            _shown.Start();
                        }
                    }
                    else if (key == Keys.PageDown)
                    {
                        if (_winKey)
                        {
                            Debug.WriteLine($"[WIN] + [PGDN]");
                            _form.UpdateAllSliderControls();
                            _form.eSetVis(true);
                            _shown.Stop();
                            _shown.Start();
                        }
                    }
                }
                else if (wParam == (IntPtr)WM_KEYUP)
                {
                    var vkCode = Marshal.ReadInt32(lParam);
                    var key = (Keys)vkCode;

                    if (key == Keys.RWin || key == Keys.LWin)
                    {
                        _winKey = false;
                        Debug.WriteLine($"WinKey! Up!");
                    }
                }
            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (var curProcess = Process.GetCurrentProcess())
                using (var curModule = curProcess.MainModule)
                    return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
        }

        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
    }
}

