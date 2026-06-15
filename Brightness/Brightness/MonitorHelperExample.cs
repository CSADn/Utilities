namespace Brightness
{
    using System;

    public class MonitorHelperExample
    {
        public static DxvaMonFn.PHYSICAL_MONITOR[] PhysicalMonitors;

        public static void Form1_FormClosing()
        {
            DxvaMonFn.DestroyAllPhysicalMonitors(PhysicalMonitors);
        }

        public static void Form1_Load()
        {
            PhysicalMonitors = DxvaMonFn.GetPhysicalMonitors_All_Flattened();
            foreach (DxvaMonFn.PHYSICAL_MONITOR physical_monitor in PhysicalMonitors)
            {
                Console.WriteLine(physical_monitor.hPhysicalMonitor.ToString() + "  " + physical_monitor.szPhysicalMonitorDescription + "\r\n");
            }
            if (PhysicalMonitors.Length != 0)
            {
                double physicalMonitorBrightness = DxvaMonFn.GetPhysicalMonitorBrightness(PhysicalMonitors[0]);
                if (!double.IsNaN(physicalMonitorBrightness))
                {
                    double num3 = physicalMonitorBrightness;
                }
            }
            if (PhysicalMonitors.Length > 1)
            {
                double physicalMonitorBrightness = DxvaMonFn.GetPhysicalMonitorBrightness(PhysicalMonitors[1]);
                if (!double.IsNaN(physicalMonitorBrightness))
                {
                    double num5 = physicalMonitorBrightness;
                }
            }
        }

        private void NumericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            if (PhysicalMonitors.Length != 0)
            {
            }
        }

        private void NumericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            if (PhysicalMonitors.Length > 1)
            {
            }
        }
    }
}

