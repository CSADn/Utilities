using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace Brightness
{


    public class RichInfoScreen
    {
        public System.Windows.Forms.Screen Screen;
        public User32_MonFn.DISPLAYCONFIG_TARGET_DEVICE_NAME? dc_TARGET_DEVICE_NAME;
        public DxvaMonFn.PHYSICAL_MONITOR[] PhysicalMonitors;
        public wmiMonFn.WMIMonitorIDObj WMIMonitorID;
        public string TooltipText = ".";

        public static List<RichInfoScreen> Get_RichInfo_Screen()
        {
            List<RichInfoScreen> list = new List<RichInfoScreen>();
            List<wmiMonFn.WMIMonitorIDObj> wMIMonitorIDs = new List<wmiMonFn.WMIMonitorIDObj>();
            try
            {
                wMIMonitorIDs = wmiMonFn.GetWMIMonitorIDs();
            }
            catch (Exception exception)
            {
                FileLogger.Log("Exception:\r\n" + exception, true, "Get_RichInfo_Screen", @"D:\e_Belgelerim\Coding_Projects\vs2017\Win10_BrightnessSlider\Win10_BrightnessSlider\Monitor\RichInfoScreen.cs", 0x7f);
            }
            foreach (System.Windows.Forms.Screen screen in System.Windows.Forms.Screen.AllScreens)
            {
                DxvaMonFn.PHYSICAL_MONITOR[] physicalMonitors = new DxvaMonFn.PHYSICAL_MONITOR[0];
                try
                {
                    physicalMonitors = DxvaMonFn.GetPhysicalMonitors(screen);
                }
                catch (Exception exception2)
                {
                    FileLogger.Log("Exception:\r\n" + exception2, true, "Get_RichInfo_Screen", @"D:\e_Belgelerim\Coding_Projects\vs2017\Win10_BrightnessSlider\Win10_BrightnessSlider\Monitor\RichInfoScreen.cs", 0x86);
                }
                wmiMonFn.WMIMonitorIDObj item = null;
                User32_MonFn.DISPLAYCONFIG_TARGET_DEVICE_NAME? nullable = screen.dc_TargetDeviceName();
                if (nullable != null)
                {
                    item = GetWmiMonitorID_by_TargetDeviceName(wMIMonitorIDs, nullable.Value);
                    wMIMonitorIDs.Remove(item);
                }
                RichInfoScreen screen1 = new RichInfoScreen();
                screen1.Screen = screen;
                screen1.dc_TARGET_DEVICE_NAME = nullable;
                screen1.PhysicalMonitors = physicalMonitors;
                screen1.WMIMonitorID = item;
                list.Add(screen1);
            }
            if (Enumerable.Count<wmiMonFn.WMIMonitorIDObj>(wMIMonitorIDs) > 0)
            {
                FileLogger.Log("[Info] WMIMonitorIDs.Count > 0: " + wMIMonitorIDs + ". Remaining wmis will be added to below. \r\nbut wmi works better if you have supported device. ", true, "Get_RichInfo_Screen", @"D:\e_Belgelerim\Coding_Projects\vs2017\Win10_BrightnessSlider\Win10_BrightnessSlider\Monitor\RichInfoScreen.cs", 160);
                list.InsertRange(0, Enumerable.ToList<RichInfoScreen>(Enumerable.Select<wmiMonFn.WMIMonitorIDObj, RichInfoScreen>(wMIMonitorIDs, delegate (wmiMonFn.WMIMonitorIDObj remWmi) {
                    RichInfoScreen screen1 = new RichInfoScreen();
                    screen1.WMIMonitorID = remWmi;
                    return screen1;
                })));
            }
            return list;
        }

        public unsafe int GetBrightness()
        {
            string instanceName;
            int brightness = -1;

            if (this.WMIMonitorID != null)
            {
                instanceName = this.WMIMonitorID.InstanceName;
            }
            else
            {
                wmiMonFn.WMIMonitorIDObj wMIMonitorID = this.WMIMonitorID;
                instanceName = null;
            }

            if (instanceName != null)
            {
                brightness = wmiMonFn.GetBrightness(this.WMIMonitorID.InstanceName);
            }

            if (brightness == -1)
            {
                brightness = DxvaMonFn.GetPhysicalMonitorBrightness(this.PhysicalMonitor.Value);
            }
            return brightness;
        }

        public static List<RichInfoScreen> GetMonitors()
        {
            List<RichInfoScreen> list = new List<RichInfoScreen>();
            DxvaMonFn.PHYSICAL_MONITOR[] physical_monitorArray = new DxvaMonFn.PHYSICAL_MONITOR[0];
            List<wmiMonFn.WMIMonitorIDObj> wMIMonitorIDs = new List<wmiMonFn.WMIMonitorIDObj>();
            bool flag = false;
            bool flag2 = false;
            try
            {
                wMIMonitorIDs = wmiMonFn.GetWMIMonitorIDs();
            }
            catch (Exception exception)
            {
                FileLogger.Log("WMI:\r\n" + exception, true, "GetMonitors", @"D:\e_Belgelerim\Coding_Projects\vs2017\Win10_BrightnessSlider\Win10_BrightnessSlider\Monitor\RichInfoScreen.cs", 210);
                flag = true;
            }
            try
            {
                physical_monitorArray = DxvaMonFn.GetPhysicalMonitors_All_Flattened();
            }
            catch (Exception exception2)
            {
                FileLogger.Log("dxva2:\r\n" + exception2, true, "GetMonitors", @"D:\e_Belgelerim\Coding_Projects\vs2017\Win10_BrightnessSlider\Win10_BrightnessSlider\Monitor\RichInfoScreen.cs", 0xd3);
                flag2 = true;
            }
            if ((Enumerable.Count<wmiMonFn.WMIMonitorIDObj>(wMIMonitorIDs) == physical_monitorArray.Length) && (Enumerable.Count<wmiMonFn.WMIMonitorIDObj>(wMIMonitorIDs) > 0))
            {
                int index = 0;
                while (true)
                {
                    if (index >= physical_monitorArray.Length)
                    {
                        break;
                    }
                    RichInfoScreen item = new RichInfoScreen {
                        WMIMonitorID = wMIMonitorIDs[index]
                    };
                    item.PhysicalMonitors = new DxvaMonFn.PHYSICAL_MONITOR[] { physical_monitorArray[index] };
                    item.TooltipText = wMIMonitorIDs[index].InstanceName;
                    item.TooltipTitle = "WMI, Fallback to dxva2";
                    list.Add(item);
                    index++;
                }
            }
            else
            {
                if (!flag)
                {
                    foreach (wmiMonFn.WMIMonitorIDObj obj2 in wMIMonitorIDs)
                    {
                        RichInfoScreen item = new RichInfoScreen {
                            WMIMonitorID = obj2,
                            TooltipText = obj2.InstanceName,
                            TooltipTitle = "WMI"
                        };
                        list.Add(item);
                    }
                }
                if (!flag2)
                {
                    int index = 0;
                    while (true)
                    {
                        if (index >= physical_monitorArray.Length)
                        {
                            break;
                        }
                        RichInfoScreen item = new RichInfoScreen();
                        item.PhysicalMonitors = new DxvaMonFn.PHYSICAL_MONITOR[] { physical_monitorArray[index] };
                        item.TooltipText = physical_monitorArray[index].hPhysicalMonitor.ToString() + "  " + physical_monitorArray[index].szPhysicalMonitorDescription;
                        item.TooltipTitle = "dxva2";
                        list.Add(item);
                        index++;
                    }
                }
            }
            return ((flag2 & flag) ? null : list);
        }

        private static wmiMonFn.WMIMonitorIDObj GetWmiMonitorID_by_TargetDeviceName(List<wmiMonFn.WMIMonitorIDObj> WMIMonitorIDs, User32_MonFn.DISPLAYCONFIG_TARGET_DEVICE_NAME dc_tarDevName)
        {
            wmiMonFn.WMIMonitorIDObj obj3;
            string dc_tarDevName_monDevPath_asInstace = "none";
            int index = dc_tarDevName.monitorDevicePath.IndexOf("DISPLAY");
            int num2 = dc_tarDevName.monitorDevicePath.IndexOf("#{");
            if (index < 0)
            {
                obj3 = null;
            }
            else
            {
                dc_tarDevName_monDevPath_asInstace = dc_tarDevName.monitorDevicePath.Substring(index, num2 - index).Replace("#", @"\");
                obj3 = Enumerable.FirstOrDefault<wmiMonFn.WMIMonitorIDObj>(WMIMonitorIDs, x => x.InstanceName.StartsWith(dc_tarDevName_monDevPath_asInstace));
            }
            return obj3;
        }

        public unsafe int SetBrightness(int value, bool _isMouseDown)
        {
            if (this.WMIMonitorID != null)
            {
                wmiMonFn.SetBrightness((byte)value, this.WMIMonitorID.InstanceName);
            }
            else
            { 
                DxvaMonFn.SetPhysicalMonitorBrightness(this.PhysicalMonitor.Value, (double)value);
            }

            return value;
        }

        public DxvaMonFn.PHYSICAL_MONITOR? PhysicalMonitor
        {
            get
            {
                DxvaMonFn.PHYSICAL_MONITOR? nullable1;
                if (this.PhysicalMonitors != null)
                {
                    nullable1 = new DxvaMonFn.PHYSICAL_MONITOR?(Enumerable.FirstOrDefault<DxvaMonFn.PHYSICAL_MONITOR>(this.PhysicalMonitors));
                }
                else
                {
                    DxvaMonFn.PHYSICAL_MONITOR[] physicalMonitors = this.PhysicalMonitors;
                    nullable1 = null;
                }
                return nullable1;
            }
        }

        public string TooltipTitle
        {
            get
            {
                string str;

                try
                {
                    string instanceName;

                    if (this.WMIMonitorID != null)
                    {
                        instanceName = this.WMIMonitorID.InstanceName;
                    }
                    else
                    {
                        wmiMonFn.WMIMonitorIDObj wMIMonitorID = this.WMIMonitorID;
                        instanceName = null;
                    }
                    if (instanceName != null)
                    {
                        str = string.IsNullOrWhiteSpace(this.WMIMonitorID.UserFriendlyName) ? ("wmi." + this.WMIMonitorID.ManufacturerName + "-" + this.WMIMonitorID.ProductCodeID) : ("wmi." + this.WMIMonitorID.UserFriendlyName);
                    }
                    else if (this.dc_TARGET_DEVICE_NAME != null)
                    {
                        User32_MonFn.DISPLAYCONFIG_TARGET_DEVICE_NAME displayconfig_target_device_name = this.dc_TARGET_DEVICE_NAME.Value;
                        object[] objArray1 = new object[] { "user32_dc.", displayconfig_target_device_name.edidManufactureId, "-", displayconfig_target_device_name.edidProductCodeId };
                        string text1 = string.Concat(objArray1);
                        str = text1 ?? "";
                    }
                    else
                    {
                        str = "noName";
                    }
                }
                catch (Exception exception)
                {
                    str = "Ex" + exception;
                }
                return str;
            }
            set
            {
                this.TooltipTitle = value;
            }
        }
    }
}

