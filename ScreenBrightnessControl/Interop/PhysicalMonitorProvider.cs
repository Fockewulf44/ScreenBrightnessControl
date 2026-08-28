using System;
using System.Collections.Generic;

namespace ScreenBrightnessControl.Interop
{
    /// <summary>
    /// Enumerates physical monitors through EnumDisplayMonitors + GetPhysicalMonitorsFromHMONITOR.
    /// </summary>
    internal sealed class PhysicalMonitorProvider : IPhysicalMonitorProvider
    {
        public IReadOnlyList<PhysicalMonitorHandle> GetPhysicalMonitors()
        {
            List<PhysicalMonitorHandle> handles = new();

            NativeMethods.MonitorEnumProc callback = (hMonitor, hdc, lprc, data) =>
            {
                CollectPhysicalMonitors(hMonitor, handles);
                return true;
            };

            try
            {
                NativeMethods.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, callback, IntPtr.Zero);
            }
            finally
            {
                // The delegate is only referenced by native code for the duration of the call.
                GC.KeepAlive(callback);
            }

            return handles;
        }

        private static void CollectPhysicalMonitors(IntPtr hMonitor, List<PhysicalMonitorHandle> handles)
        {
            if (!NativeMethods.GetNumberOfPhysicalMonitorsFromHMONITOR(hMonitor, out uint count) || count == 0)
            {
                return;
            }

            NativeMethods.PHYSICAL_MONITOR[] buffer = new NativeMethods.PHYSICAL_MONITOR[count];

            if (!NativeMethods.GetPhysicalMonitorsFromHMONITOR(hMonitor, count, buffer))
            {
                return;
            }

            foreach (NativeMethods.PHYSICAL_MONITOR monitor in buffer)
            {
                handles.Add(new PhysicalMonitorHandle(monitor.hPhysicalMonitor, monitor.szPhysicalMonitorDescription));
            }
        }
    }
}
