using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Management;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Brightness
{
    public static class HelperFn
    {
        public static bool ApplicationIsActivated()
        {
            bool flag2;
            IntPtr foregroundWindow = GetForegroundWindow();
            if (foregroundWindow == IntPtr.Zero)
            {
                flag2 = false;
            }
            else
            {
                int num2;
                GetWindowThreadProcessId(foregroundWindow, out num2);
                flag2 = num2 == Process.GetCurrentProcess().Id;
            }
            return flag2;
        }

        public static Point CalculateFormLocationNearToTaskbarDatePart(Size formSize)
        {
            Point point = new Point(0, 0);
            Rectangle workingArea = Screen.PrimaryScreen.WorkingArea;
            TaskBarLocation taskBarLocation = GetTaskBarLocation();
            if (!CultureInfo.CurrentCulture.TextInfo.IsRightToLeft)
            {
                if (taskBarLocation == TaskBarLocation.TOP)
                {
                    point = new Point(workingArea.Width - formSize.Width, workingArea.Top);
                }
                else if (taskBarLocation == TaskBarLocation.LEFT)
                {
                    point = new Point(workingArea.Left, workingArea.Height - formSize.Height);
                }
                else if (taskBarLocation == TaskBarLocation.BOTTOM)
                {
                    point = new Point(workingArea.Width - formSize.Width, workingArea.Height - formSize.Height);
                }
                else if (taskBarLocation == TaskBarLocation.RIGHT)
                {
                    point = new Point(workingArea.Width - formSize.Width, workingArea.Height - formSize.Height);
                }
            }
            else if (taskBarLocation == TaskBarLocation.TOP)
            {
                point = new Point(0, workingArea.Top);
            }
            else if (taskBarLocation == TaskBarLocation.LEFT)
            {
                point = new Point(workingArea.Left, 0);
            }
            else if (taskBarLocation == TaskBarLocation.BOTTOM)
            {
                point = new Point(0, workingArea.Height - formSize.Height);
            }
            else if (taskBarLocation == TaskBarLocation.RIGHT)
            {
                point = new Point(workingArea.Right - formSize.Width, workingArea.Top);
            }
            return point;
        }

        public static void ColorFormIfActive_inDbgMode(Form f)
        {
            if (Debugger.IsAttached)
            {
                f.BackColor = ApplicationIsActivated() ? System.Drawing.Color.DarkRed : System.Drawing.Color.Black;
            }
        }

        public static string GetActiveProcessFileName()
        {
            uint num;
            GetWindowThreadProcessId(GetForegroundWindow(), out num);
            return Process.GetProcessById((int) num).ProcessName;
        }

        public static List<string> GetAllMonitorInfo()
        {
            List<string> list = new List<string>();
            foreach (ManagementObject obj2 in new ManagementObjectSearcher(@"root\WMI", "SELECT * FROM WmiMonitorBasicDisplayParams").Get())
            {
                string[] textArray1 = new string[13];
                textArray1[0] = obj2["InstanceName"].ToString();
                textArray1[1] = "\r\n";
                textArray1[2] = obj2["Active"].ToString();
                textArray1[3] = "\r\n";
                textArray1[4] = obj2["DisplayTransferCharacteristic"].ToString();
                textArray1[5] = "\r\n";
                textArray1[6] = obj2["MaxHorizontalImageSize"].ToString();
                textArray1[7] = "\r\n";
                textArray1[8] = obj2["MaxVerticalImageSize"].ToString();
                textArray1[9] = "\r\n";
                textArray1[10] = obj2["SupportedDisplayFeatures"].ToString();
                textArray1[11] = "\r\n";
                textArray1[12] = obj2["VideoInputType"].ToString();
                string item = string.Concat(textArray1);
                list.Add(item);
            }
            return list;
        }

        public static List<string> GetAllScreensInfo()
        {
            List<string> list = new List<string>();
            foreach (Screen screen in Screen.AllScreens)
            {
                string[] textArray1 = new string[11];
                textArray1[0] = "Device Name: ";
                textArray1[1] = screen.DeviceName;
                textArray1[2] = "\r\nBounds: ";
                textArray1[3] = screen.Bounds.ToString();
                textArray1[4] = "\r\nType: ";
                textArray1[5] = screen.GetType().ToString();
                textArray1[6] = "\r\nWorking Area: ";
                textArray1[7] = screen.WorkingArea.ToString();
                textArray1[8] = "\r\nPrimary Screen: ";
                textArray1[9] = screen.Primary.ToString();
                textArray1[10] = "\r\n";
                string item = string.Concat(textArray1);
                list.Add(item);
            }
            return list;
        }

        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        private static extern IntPtr GetForegroundWindow();
        public static TaskBarLocation GetTaskBarLocation()
        {
            TaskBarLocation bOTTOM = TaskBarLocation.BOTTOM;
            if (Screen.PrimaryScreen.WorkingArea.Width == Screen.PrimaryScreen.Bounds.Width)
            {
                bOTTOM = (Screen.PrimaryScreen.WorkingArea.Top <= 0) ? TaskBarLocation.BOTTOM : TaskBarLocation.TOP;
            }
            else
            {
                bOTTOM = (Screen.PrimaryScreen.WorkingArea.Left <= 0) ? TaskBarLocation.RIGHT : TaskBarLocation.LEFT;
            }
            return bOTTOM;
        }

        [DllImport("user32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        private static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out uint ProcessId);
        public static bool isRunAtStartup() => 
            ((Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true).GetValue(Application.ProductName)) == Application.ExecutablePath);

        public static void SetStartup(bool RunAtStartup)
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
            if (RunAtStartup)
            {
                key.SetValue(Application.ProductName, Application.ExecutablePath);
            }
            else
            {
                key.DeleteValue(Application.ProductName, false);
            }
        }

        public static void SetTooltip(Control ctl, string text, string title = "")
        {
            new ToolTip { 
                ToolTipIcon = ToolTipIcon.Info,
                ShowAlways = true,
                ToolTipTitle = title
            }.SetToolTip(ctl, text);
        }

        public enum TaskBarLocation
        {
            TOP,
            BOTTOM,
            LEFT,
            RIGHT
        }
    }
}

