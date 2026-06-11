using System;
using System.Runtime.InteropServices;
using System.Management;
using System.Diagnostics;

namespace ScreenBrightnessControl
{
    public class ScreenBrightnessControl
    {

        [DllImport("dxva2.dll", SetLastError = true)]
        public static extern bool SetMonitorBrightness(IntPtr hMonitor, uint newBrightness);

        [DllImport("dxva2.dll", SetLastError = true)]
        public static extern bool GetMonitorBrightness(IntPtr hMonitor, out uint min, out uint current, out uint max);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip,
            MonitorEnumProc lpfnEnum, IntPtr dwData);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

        [DllImport("dxva2.dll", SetLastError = true)]
        public static extern bool GetPhysicalMonitorsFromHMONITOR(IntPtr hMonitor,
            uint physicalMonitorArraySize, [Out] PHYSICAL_MONITOR[] physicalMonitorArray);

        [DllImport("dxva2.dll", SetLastError = true)]
        public static extern bool DestroyPhysicalMonitor(IntPtr hMonitor);

        public delegate bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdcMonitor, IntPtr lprcMonitor, IntPtr dwData);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct PHYSICAL_MONITOR
        {
            public IntPtr hPhysicalMonitor;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string szPhysicalMonitorDescription;
        }

        public static int GetLaptopScreenBrightness()
        {
            using var mclass = new ManagementClass("WmiMonitorBrightness")
            {
                Scope = new ManagementScope(@"\\.\root\wmi")
            };
            using var instances = mclass.GetInstances();
            foreach (ManagementObject instance in instances)
            {
                return (byte)instance.GetPropertyValue("CurrentBrightness");
            }
            return 0;
        }

        public static void SetLaptopScreenBrightness(int brightness)
        {
            using var mclass = new ManagementClass("WmiMonitorBrightnessMethods")
            {
                Scope = new ManagementScope(@"\\.\root\wmi")
            };
            using var instances = mclass.GetInstances();
            object[] args = new object[] { 1, brightness };
            foreach (ManagementObject instance in instances)
            {
                instance.InvokeMethod("WmiSetBrightness", args);
            }
        }

        public static void GetMonitorBrightness()
        {
            EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, (hMonitor, hdc, rc, data) =>
            {
                PHYSICAL_MONITOR[] monitors = new PHYSICAL_MONITOR[1];

                if (GetPhysicalMonitorsFromHMONITOR(hMonitor, 1, monitors))
                {
                    var mon = monitors[0];
                    Debug.WriteLine("Monitor: " + mon.szPhysicalMonitorDescription);

                    if (GetMonitorBrightness(mon.hPhysicalMonitor, out uint min, out uint current, out uint max))
                    {
                        Debug.WriteLine($"Current brightness: {current}");
                    }

                    DestroyPhysicalMonitor(mon.hPhysicalMonitor);
                }

                return true;
            }, IntPtr.Zero);
        }

        public static void SetMonitorBrightness(int brightness)
        {
            EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, (hMonitor, hdc, rc, data) =>
            {
                PHYSICAL_MONITOR[] monitors = new PHYSICAL_MONITOR[1];

                if (GetPhysicalMonitorsFromHMONITOR(hMonitor, 1, monitors))
                {
                    var mon = monitors[0];                    
                    Debug.WriteLine("Monitor: " + mon.szPhysicalMonitorDescription);

                    if (GetMonitorBrightness(mon.hPhysicalMonitor, out uint min, out uint current, out uint max))
                    {
                        Debug.WriteLine($"Current brightness: {current}");                        
                        SetMonitorBrightness(mon.hPhysicalMonitor, (uint)brightness);

                        Debug.WriteLine("Brightness set to " + brightness.ToString());
                    }

                    DestroyPhysicalMonitor(mon.hPhysicalMonitor);
                }

                return true;
            }, IntPtr.Zero);
        }
    }
}
