using System;
using System.Collections.Generic;
using System.Management;

namespace Brightness
{


    public static class wmiMonFn
    {
        public static int GetBrightness(string wmi_InstanceName)
        {
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(new ManagementScope(@"root\WMI"), new SelectQuery("WmiMonitorBrightness")))
                {
                    using (ManagementObjectCollection objects = searcher.Get())
                    {
                        using (ManagementObjectCollection.ManagementObjectEnumerator enumerator = objects.GetEnumerator())
                        {
                            while (true)
                            {
                                if (!enumerator.MoveNext())
                                {
                                    break;
                                }
                                ManagementObject current = (ManagementObject) enumerator.Current;
                                string str = current.Properties["InstanceName"].Value.ToString();
                                if (wmi_InstanceName == str)
                                {
                                    float result = -1f;
                                    float.TryParse(current.Properties["CurrentBrightness"].Value.ToString(), out result);
                                    return (int) result;
                                }
                            }
                        }
                    }
                }
                return -1;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public static List<WMIMonitorIDObj> GetWMIMonitorIDs()
        {
            List<WMIMonitorIDObj> list = new List<WMIMonitorIDObj>();
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(new ManagementScope(@"root\WMI"), new SelectQuery("WMIMonitorID")))
            {
                using (ManagementObjectCollection objects = searcher.Get())
                {
                    foreach (ManagementObject obj2 in objects)
                    {
                        WMIMonitorIDObj item = new WMIMonitorIDObj {
                            InstanceName = obj2.Properties["InstanceName"].Value.ToString(),
                            ManufacturerName = ToCharArray((ushort[]) obj2.Properties["ManufacturerName"].Value),
                            ProductCodeID = ToCharArray((ushort[]) obj2.Properties["ProductCodeID"].Value),
                            SerialNumberID = ToCharArray((ushort[]) obj2.Properties["SerialNumberID"].Value),
                            UserFriendlyName = ToCharArray((ushort[]) obj2.Properties["UserFriendlyName"].Value),
                            YearOfManufacture = obj2.Properties["YearOfManufacture"].Value.ToString()
                        };
                        list.Add(item);
                    }
                }
            }
            return list;
        }

        public static bool SetBrightness(byte value, string wmi_InstanceName)
        {
            bool flag = false;
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(new ManagementScope(@"root\WMI"), new SelectQuery("WmiMonitorBrightnessMethods")))
                {
                    using (ManagementObjectCollection objects = searcher.Get())
                    {
                        foreach (ManagementObject obj2 in objects)
                        {
                            string str = obj2.Properties["InstanceName"].Value.ToString();
                            if (wmi_InstanceName == str)
                            {
                                object[] args = new object[] { uint.MaxValue, value };
                                obj2.InvokeMethod("WmiSetBrightness", args);
                                flag = true;
                                break;
                            }
                        }
                    }
                }
                return flag;
            }
            catch (Exception exception1)
            {
                Console.WriteLine(exception1.Message);
                return false;
            }
        }

        public static string ToCharArray(ushort[] arr)
        {
            string str;
            if (arr == null)
            {
                str = "";
            }
            else
            {
                char[] chArray = new char[arr.Length];
                int index = 0;
                while (true)
                {
                    if (index >= arr.Length)
                    {
                        str = new string(chArray).TrimEnd(new char[1]);
                        break;
                    }
                    chArray[index] = (char) arr[index];
                    index++;
                }
            }
            return str;
        }

        public class WMIMonitorIDObj
        {
            public string InstanceName;
            public string ManufacturerName;
            public string ProductCodeID;
            public string SerialNumberID;
            public string UserFriendlyName;
            public string YearOfManufacture;
        }
    }
}

