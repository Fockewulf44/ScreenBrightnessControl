using System;
using System.Collections.Generic;
using System.Diagnostics;
using ScreenBrightnessControl.Interop;

namespace ScreenBrightnessControl.Services
{
    /// <summary>
    /// Drives external monitors over DDC/CI. Monitors that do not answer DDC/CI are skipped.
    /// </summary>
    public sealed class ExternalMonitorBrightnessController : IBrightnessController
    {
        private readonly IPhysicalMonitorProvider _monitorProvider;

        internal ExternalMonitorBrightnessController(IPhysicalMonitorProvider monitorProvider)
        {
            _monitorProvider = monitorProvider ?? throw new ArgumentNullException(nameof(monitorProvider));
        }

        public ExternalMonitorBrightnessController()
            : this(new PhysicalMonitorProvider())
        {
        }

        public string DisplayName => "Monitor Brightness";

        public BrightnessRange? TryReadRange()
        {
            BrightnessRange? range = null;

            ForEachMonitor((handle, current) =>
            {
                range ??= current;
            });

            return range;
        }

        public void SetBrightness(int brightness)
        {
            ForEachMonitor((handle, current) =>
            {
                uint target = (uint)current.Clamp(brightness);

                if (!NativeMethods.SetMonitorBrightness(handle.Handle, target))
                {
                    Debug.WriteLine($"'{handle.Description}' refused brightness {target}.");
                }
            });
        }

        /// <summary>
        /// Runs <paramref name="action"/> against every monitor that reports a brightness range,
        /// guaranteeing that all physical monitor handles are released afterwards.
        /// </summary>
        private void ForEachMonitor(Action<PhysicalMonitorHandle, BrightnessRange> action)
        {
            IReadOnlyList<PhysicalMonitorHandle> monitors = _monitorProvider.GetPhysicalMonitors();

            try
            {
                foreach (PhysicalMonitorHandle monitor in monitors)
                {
                    if (TryReadRange(monitor, out BrightnessRange range))
                    {
                        action(monitor, range);
                    }
                }
            }
            finally
            {
                foreach (PhysicalMonitorHandle monitor in monitors)
                {
                    monitor.Dispose();
                }
            }
        }

        private static bool TryReadRange(PhysicalMonitorHandle monitor, out BrightnessRange range)
        {
            if (NativeMethods.GetMonitorBrightness(monitor.Handle, out uint minimum, out uint current, out uint maximum)
                && maximum > minimum)
            {
                range = new BrightnessRange((int)minimum, (int)maximum, (int)current);
                return true;
            }

            Debug.WriteLine($"'{monitor.Description}' does not report a DDC/CI brightness range.");
            range = default;
            return false;
        }
    }
}
