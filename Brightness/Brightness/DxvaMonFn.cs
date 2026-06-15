namespace Brightness
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Windows.Forms;

    public static class DxvaMonFn
    {
        public static bool DestroyAllPhysicalMonitors(PHYSICAL_MONITOR[] PhysicalMonitors) => 
            DestroyPhysicalMonitors(Convert.ToUInt32(PhysicalMonitors.Length), PhysicalMonitors);

        [DllImport("dxva2.dll", SetLastError=true)]
        public static extern bool DestroyPhysicalMonitors(uint dwPhysicalMonitorArraySize, PHYSICAL_MONITOR[] pPhysicalMonitorArray);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("dxva2.dll", SetLastError=true)]
        public static extern bool GetMonitorBrightness(IntPtr handle, out uint minBrightness, out uint currentBrightness, out uint maxBrightness);
        [DllImport("dxva2.dll", SetLastError=true)]
        public static extern bool GetMonitorCapabilities(IntPtr hMonitor, ref uint pdwMonitorCapabilities, ref uint pdwSupportedColorTemperatures);
        [DllImport("dxva2.dll", SetLastError=true)]
        public static extern bool GetMonitorContrast(IntPtr hMonitor, ref uint pdwMinContrast, ref uint pdwCurrentContrast, ref uint pdwMaxContrast);
        [DllImport("dxva2.dll", SetLastError=true)]
        public static extern bool GetNumberOfPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, ref uint pdwNumberOfPhysicalMonitors);
        public static int GetPhysicalMonitorBrightness(PHYSICAL_MONITOR physicalMonitor)
        {
            int num4;
            uint minBrightness = 0;
            uint currentBrightness = 0;
            uint maxBrightness = 0;
            if (GetMonitorBrightness(physicalMonitor.hPhysicalMonitor, out minBrightness, out currentBrightness, out maxBrightness))
            {
                num4 = (int)((currentBrightness - minBrightness) / (maxBrightness - minBrightness) * 100.0);
            }
            else
            {
                Console.WriteLine(new Win32Exception(Marshal.GetLastWin32Error()));
                num4 = -1;
            }
            return num4;
        }

        private static uint GetPhysicalMonitorCapabilities(PHYSICAL_MONITOR physicalMonitor)
        {
            uint pdwMonitorCapabilities = 0;
            uint pdwSupportedColorTemperatures = 0;
            GetMonitorCapabilities(physicalMonitor.hPhysicalMonitor, ref pdwMonitorCapabilities, ref pdwSupportedColorTemperatures);
            return pdwMonitorCapabilities;
        }

        public static PHYSICAL_MONITOR[] GetPhysicalMonitors(Screen screen)
        {
            IntPtr hMonitor = MonitorFromPoint(screen.Bounds.Location, MonitorOptions.MONITOR_DEFAULTTONEAREST);
            uint pdwNumberOfPhysicalMonitors = 0;
            GetNumberOfPhysicalMonitorsFromHMONITOR(hMonitor, ref pdwNumberOfPhysicalMonitors);
            PHYSICAL_MONITOR[] pPhysicalMonitorArray = new PHYSICAL_MONITOR[pdwNumberOfPhysicalMonitors];
            GetPhysicalMonitorsFromHMONITOR(hMonitor, pdwNumberOfPhysicalMonitors, pPhysicalMonitorArray);
            return pPhysicalMonitorArray;
        }

        public static PHYSICAL_MONITOR[] GetPhysicalMonitors_All_Flattened()
        {
            List<PHYSICAL_MONITOR> list = new List<PHYSICAL_MONITOR>();
            foreach (Screen screen in Screen.AllScreens)
            {
                list.AddRange(GetPhysicalMonitors(screen));
            }
            return list.ToArray();
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("dxva2.dll")]
        public static extern bool GetPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, uint dwPhysicalMonitorArraySize, [Out] PHYSICAL_MONITOR[] pPhysicalMonitorArray);
        [DllImport("user32.dll", SetLastError=true)]
        public static extern IntPtr MonitorFromPoint(Point pt, MonitorOptions dwFlags);
        [DllImport("dxva2.dll", SetLastError=true)]
        public static extern bool SetMonitorBrightness(IntPtr hMonitor, uint dwNewBrightness);
        [DllImport("dxva2.dll", SetLastError=true)]
        public static extern bool SetMonitorContrast(IntPtr hMonitor, uint dwNewContrast);
        public static bool SetPhysicalMonitorBrightness(PHYSICAL_MONITOR physicalMonitor, double brightness)
        {
            bool flag2;
            SetMonitorBrightness(physicalMonitor.hPhysicalMonitor, Convert.ToUInt32(brightness));
            Thread.Sleep(60);
            uint minBrightness = 0;
            uint currentBrightness = 0;
            uint maxBrightness = 0;
            if (!GetMonitorBrightness(physicalMonitor.hPhysicalMonitor, out minBrightness, out currentBrightness, out maxBrightness))
            {
                Console.WriteLine(new Win32Exception(Marshal.GetLastWin32Error()));
                flag2 = false;
            }
            else
            {
                var value = (minBrightness + ((maxBrightness - minBrightness) * (brightness / 100.0)));
                if (SetMonitorBrightness(physicalMonitor.hPhysicalMonitor, Convert.ToUInt32(value)))
                {
                    flag2 = true;
                }
                else
                {
                    Console.WriteLine(new Win32Exception(Marshal.GetLastWin32Error()));
                    flag2 = false;
                }
            }
            return flag2;
        }

        public enum MonitorOptions : uint
        {
            MONITOR_DEFAULTTONULL = 0,
            MONITOR_DEFAULTTOPRIMARY = 1,
            MONITOR_DEFAULTTONEAREST = 2
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        public struct PHYSICAL_MONITOR
        {
            public IntPtr hPhysicalMonitor;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=0x80)]
            public string szPhysicalMonitorDescription;
        }
    }
}

