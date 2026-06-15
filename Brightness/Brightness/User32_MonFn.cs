using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Brightness
{


    public static class User32_MonFn
    {
        public const int ERROR_SUCCESS = 0;

        public static DISPLAYCONFIG_TARGET_DEVICE_NAME? dc_TargetDeviceName(this Screen screen)
        {
            IEnumerable<DISPLAYCONFIG_TARGET_DEVICE_NAME> allMonitorsFriendlyNames = GetAllMonitorsFriendlyNames();
            int index = 0;
            while (true)
            {
                DISPLAYCONFIG_TARGET_DEVICE_NAME? nullable;
                if (index >= Screen.AllScreens.Length)
                {
                    nullable = null;
                }
                else
                {
                    if (!Equals(screen, Screen.AllScreens[index]))
                    {
                        index++;
                        continue;
                    }
                    nullable = new DISPLAYCONFIG_TARGET_DEVICE_NAME?(Enumerable.ToArray<DISPLAYCONFIG_TARGET_DEVICE_NAME>(allMonitorsFriendlyNames)[index]);
                }
                return nullable;
            }
        }

        [DllImport("user32.dll")]
        public static extern int DisplayConfigGetDeviceInfo(ref DISPLAYCONFIG_TARGET_DEVICE_NAME deviceName);

        private static IEnumerable<DISPLAYCONFIG_TARGET_DEVICE_NAME> GetAllMonitorsFriendlyNames()
        {
            uint pathCount, modeCount;
            var error = GetDisplayConfigBufferSizes(QUERY_DEVICE_CONFIG_FLAGS.QDC_ONLY_ACTIVE_PATHS, out pathCount, out modeCount);
            if (error != ERROR_SUCCESS)
                throw new Win32Exception(error);

            var displayPaths = new DISPLAYCONFIG_PATH_INFO[pathCount];
            var displayModes = new DISPLAYCONFIG_MODE_INFO[modeCount];
            error = QueryDisplayConfig(QUERY_DEVICE_CONFIG_FLAGS.QDC_ONLY_ACTIVE_PATHS,
                ref pathCount, displayPaths, ref modeCount, displayModes, IntPtr.Zero);
            if (error != ERROR_SUCCESS)
                throw new Win32Exception(error);

            for (var i = 0; i < modeCount; i++)
                if (displayModes[i].infoType == DISPLAYCONFIG_MODE_INFO_TYPE.DISPLAYCONFIG_MODE_INFO_TYPE_TARGET)
                    yield return MonitorFriendlyName(displayModes[i].adapterId, displayModes[i].id);
        }

        [DllImport("user32.dll")]
        public static extern int GetDisplayConfigBufferSizes(QUERY_DEVICE_CONFIG_FLAGS flags, out uint numPathArrayElements, out uint numModeInfoArrayElements);
        private static DISPLAYCONFIG_TARGET_DEVICE_NAME MonitorFriendlyName(LUID adapterId, uint targetId)
        {
            DISPLAYCONFIG_TARGET_DEVICE_NAME deviceName = new DISPLAYCONFIG_TARGET_DEVICE_NAME {
                header = { 
                    size = (uint) Marshal.SizeOf(typeof(DISPLAYCONFIG_TARGET_DEVICE_NAME)),
                    adapterId = adapterId,
                    id = targetId,
                    type = DISPLAYCONFIG_DEVICE_INFO_TYPE.DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_NAME
                }
            };
            int error = DisplayConfigGetDeviceInfo(ref deviceName);
            if (error != 0)
            {
                throw new Win32Exception(error);
            }
            return deviceName;
        }

        [DllImport("user32.dll")]
        public static extern int QueryDisplayConfig(QUERY_DEVICE_CONFIG_FLAGS flags, ref uint numPathArrayElements, [Out] DISPLAYCONFIG_PATH_INFO[] PathInfoArray, ref uint numModeInfoArrayElements, [Out] DISPLAYCONFIG_MODE_INFO[] ModeInfoArray, IntPtr currentTopologyId);

        [StructLayout(LayoutKind.Sequential)]
        public struct DISPLAYCONFIG_2DREGION
        {
            public uint cx;
            public uint cy;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DISPLAYCONFIG_DEVICE_INFO_HEADER
        {
            public User32_MonFn.DISPLAYCONFIG_DEVICE_INFO_TYPE type;
            public uint size;
            public User32_MonFn.LUID adapterId;
            public uint id;
        }

        public enum DISPLAYCONFIG_DEVICE_INFO_TYPE : uint
        {
            DISPLAYCONFIG_DEVICE_INFO_GET_SOURCE_NAME = 1,
            DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_NAME = 2,
            DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_PREFERRED_MODE = 3,
            DISPLAYCONFIG_DEVICE_INFO_GET_ADAPTER_NAME = 4,
            DISPLAYCONFIG_DEVICE_INFO_SET_TARGET_PERSISTENCE = 5,
            DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_BASE_TYPE = 6,
            DISPLAYCONFIG_DEVICE_INFO_FORCE_UINT32 = 0xffffffff
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DISPLAYCONFIG_MODE_INFO
        {
            public User32_MonFn.DISPLAYCONFIG_MODE_INFO_TYPE infoType;
            public uint id;
            public User32_MonFn.LUID adapterId;
            public User32_MonFn.DISPLAYCONFIG_MODE_INFO_UNION modeInfo;
        }

        public enum DISPLAYCONFIG_MODE_INFO_TYPE : uint
        {
            DISPLAYCONFIG_MODE_INFO_TYPE_SOURCE = 1,
            DISPLAYCONFIG_MODE_INFO_TYPE_TARGET = 2,
            DISPLAYCONFIG_MODE_INFO_TYPE_FORCE_UINT32 = 0xffffffff
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct DISPLAYCONFIG_MODE_INFO_UNION
        {
            [FieldOffset(0)]
            public User32_MonFn.DISPLAYCONFIG_TARGET_MODE targetMode;
            [FieldOffset(0)]
            public User32_MonFn.DISPLAYCONFIG_SOURCE_MODE sourceMode;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DISPLAYCONFIG_PATH_INFO
        {
            public User32_MonFn.DISPLAYCONFIG_PATH_SOURCE_INFO sourceInfo;
            public User32_MonFn.DISPLAYCONFIG_PATH_TARGET_INFO targetInfo;
            public uint flags;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DISPLAYCONFIG_PATH_SOURCE_INFO
        {
            public User32_MonFn.LUID adapterId;
            public uint id;
            public uint modeInfoIdx;
            public uint statusFlags;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DISPLAYCONFIG_PATH_TARGET_INFO
        {
            public User32_MonFn.LUID adapterId;
            public uint id;
            public uint modeInfoIdx;
            private User32_MonFn.DISPLAYCONFIG_VIDEO_OUTPUT_TECHNOLOGY outputTechnology;
            private User32_MonFn.DISPLAYCONFIG_ROTATION rotation;
            private User32_MonFn.DISPLAYCONFIG_SCALING scaling;
            private User32_MonFn.DISPLAYCONFIG_RATIONAL refreshRate;
            private User32_MonFn.DISPLAYCONFIG_SCANLINE_ORDERING scanLineOrdering;
            public bool targetAvailable;
            public uint statusFlags;
        }

        public enum DISPLAYCONFIG_PIXELFORMAT : uint
        {
            DISPLAYCONFIG_PIXELFORMAT_8BPP = 1,
            DISPLAYCONFIG_PIXELFORMAT_16BPP = 2,
            DISPLAYCONFIG_PIXELFORMAT_24BPP = 3,
            DISPLAYCONFIG_PIXELFORMAT_32BPP = 4,
            DISPLAYCONFIG_PIXELFORMAT_NONGDI = 5,
            DISPLAYCONFIG_PIXELFORMAT_FORCE_UINT32 = 0xffffffff
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DISPLAYCONFIG_RATIONAL
        {
            public uint Numerator;
            public uint Denominator;
        }

        public enum DISPLAYCONFIG_ROTATION : uint
        {
            DISPLAYCONFIG_ROTATION_IDENTITY = 1,
            DISPLAYCONFIG_ROTATION_ROTATE90 = 2,
            DISPLAYCONFIG_ROTATION_ROTATE180 = 3,
            DISPLAYCONFIG_ROTATION_ROTATE270 = 4,
            DISPLAYCONFIG_ROTATION_FORCE_UINT32 = 0xffffffff
        }

        public enum DISPLAYCONFIG_SCALING : uint
        {
            DISPLAYCONFIG_SCALING_IDENTITY = 1,
            DISPLAYCONFIG_SCALING_CENTERED = 2,
            DISPLAYCONFIG_SCALING_STRETCHED = 3,
            DISPLAYCONFIG_SCALING_ASPECTRATIOCENTEREDMAX = 4,
            DISPLAYCONFIG_SCALING_CUSTOM = 5,
            DISPLAYCONFIG_SCALING_PREFERRED = 0x80,
            DISPLAYCONFIG_SCALING_FORCE_UINT32 = 0xffffffff
        }

        public enum DISPLAYCONFIG_SCANLINE_ORDERING : uint
        {
            DISPLAYCONFIG_SCANLINE_ORDERING_UNSPECIFIED = 0,
            DISPLAYCONFIG_SCANLINE_ORDERING_PROGRESSIVE = 1,
            DISPLAYCONFIG_SCANLINE_ORDERING_INTERLACED = 2,
            DISPLAYCONFIG_SCANLINE_ORDERING_INTERLACED_UPPERFIELDFIRST = 2,
            DISPLAYCONFIG_SCANLINE_ORDERING_INTERLACED_LOWERFIELDFIRST = 3,
            DISPLAYCONFIG_SCANLINE_ORDERING_FORCE_UINT32 = 0xffffffff
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DISPLAYCONFIG_SOURCE_MODE
        {
            public uint width;
            public uint height;
            public User32_MonFn.DISPLAYCONFIG_PIXELFORMAT pixelFormat;
            public User32_MonFn.POINTL position;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        public struct DISPLAYCONFIG_TARGET_DEVICE_NAME
        {
            public User32_MonFn.DISPLAYCONFIG_DEVICE_INFO_HEADER header;
            public User32_MonFn.DISPLAYCONFIG_TARGET_DEVICE_NAME_FLAGS flags;
            public User32_MonFn.DISPLAYCONFIG_VIDEO_OUTPUT_TECHNOLOGY outputTechnology;
            public ushort edidManufactureId;
            public ushort edidProductCodeId;
            public uint connectorInstance;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=0x40)]
            public string monitorFriendlyDeviceName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=0x80)]
            public string monitorDevicePath;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DISPLAYCONFIG_TARGET_DEVICE_NAME_FLAGS
        {
            public uint value;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DISPLAYCONFIG_TARGET_MODE
        {
            public User32_MonFn.DISPLAYCONFIG_VIDEO_SIGNAL_INFO targetVideoSignalInfo;
        }

        public enum DISPLAYCONFIG_VIDEO_OUTPUT_TECHNOLOGY : uint
        {
            DISPLAYCONFIG_OUTPUT_TECHNOLOGY_OTHER = 0xffffffff,
            DISPLAYCONFIG_OUTPUT_TECHNOLOGY_HD15 = 0,
            DISPLAYCONFIG_OUTPUT_TECHNOLOGY_SVIDEO = 1,
            DISPLAYCONFIG_OUTPUT_TECHNOLOGY_COMPOSITE_VIDEO = 2,
            DISPLAYCONFIG_OUTPUT_TECHNOLOGY_COMPONENT_VIDEO = 3,
            DISPLAYCONFIG_OUTPUT_TECHNOLOGY_DVI = 4,
            DISPLAYCONFIG_OUTPUT_TECHNOLOGY_HDMI = 5,
            DISPLAYCONFIG_OUTPUT_TECHNOLOGY_LVDS = 6,
            DISPLAYCONFIG_OUTPUT_TECHNOLOGY_D_JPN = 8,
            DISPLAYCONFIG_OUTPUT_TECHNOLOGY_SDI = 9,
            DISPLAYCONFIG_OUTPUT_TECHNOLOGY_DISPLAYPORT_EXTERNAL = 10,
            DISPLAYCONFIG_OUTPUT_TECHNOLOGY_DISPLAYPORT_EMBEDDED = 11,
            DISPLAYCONFIG_OUTPUT_TECHNOLOGY_UDI_EXTERNAL = 12,
            DISPLAYCONFIG_OUTPUT_TECHNOLOGY_UDI_EMBEDDED = 13,
            DISPLAYCONFIG_OUTPUT_TECHNOLOGY_SDTVDONGLE = 14,
            DISPLAYCONFIG_OUTPUT_TECHNOLOGY_MIRACAST = 15,
            DISPLAYCONFIG_OUTPUT_TECHNOLOGY_INTERNAL = 0x80000000,
            DISPLAYCONFIG_OUTPUT_TECHNOLOGY_FORCE_UINT32 = 0xffffffff
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DISPLAYCONFIG_VIDEO_SIGNAL_INFO
        {
            public ulong pixelRate;
            public User32_MonFn.DISPLAYCONFIG_RATIONAL hSyncFreq;
            public User32_MonFn.DISPLAYCONFIG_RATIONAL vSyncFreq;
            public User32_MonFn.DISPLAYCONFIG_2DREGION activeSize;
            public User32_MonFn.DISPLAYCONFIG_2DREGION totalSize;
            public uint videoStandard;
            public User32_MonFn.DISPLAYCONFIG_SCANLINE_ORDERING scanLineOrdering;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct LUID
        {
            public uint LowPart;
            public int HighPart;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINTL
        {
            private int x;
            private int y;
        }

        public enum QUERY_DEVICE_CONFIG_FLAGS : uint
        {
            QDC_ALL_PATHS = 1,
            QDC_ONLY_ACTIVE_PATHS = 2,
            QDC_DATABASE_CURRENT = 4
        }
    }
}

