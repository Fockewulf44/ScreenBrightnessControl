using System;
using System.Diagnostics;
using System.Management;
using System.Runtime.InteropServices;

namespace ScreenBrightnessControl.Services
{
    /// <summary>
    /// Drives the built-in laptop panel through the WMI brightness classes.
    /// </summary>
    public sealed class LaptopBrightnessController : IBrightnessController
    {
        private const string WmiScope = @"root\WMI";
        private const string BrightnessQuery = "SELECT * FROM WmiMonitorBrightness";
        private const string BrightnessMethodsQuery = "SELECT * FROM WmiMonitorBrightnessMethods";
        private const string SetBrightnessMethod = "WmiSetBrightness";

        /// <summary>
        /// Seconds WMI is allowed to spend applying the change before it gives up.
        /// </summary>
        private const uint SetBrightnessTimeoutSeconds = 1;

        public string DisplayName => "Laptop Brightness";

        public BrightnessRange? TryReadRange()
        {
            try
            {
                using ManagementObjectSearcher searcher = new(WmiScope, BrightnessQuery);
                using ManagementObjectCollection results = searcher.Get();

                foreach (ManagementBaseObject result in results)
                {
                    using (result)
                    {
                        return ReadRange(result);
                    }
                }
            }
            catch (Exception exception) when (IsExpectedWmiFailure(exception))
            {
                Debug.WriteLine($"Reading laptop brightness failed: {exception.Message}");
            }

            return null;
        }

        public void SetBrightness(int brightness)
        {
            try
            {
                using ManagementObjectSearcher searcher = new(WmiScope, BrightnessMethodsQuery);
                using ManagementObjectCollection results = searcher.Get();

                foreach (ManagementBaseObject result in results)
                {
                    using (result)
                    {
                        if (result is ManagementObject instance)
                        {
                            object[] arguments = { SetBrightnessTimeoutSeconds, (byte)Math.Clamp(brightness, 0, 100) };
                            instance.InvokeMethod(SetBrightnessMethod, arguments);
                        }
                    }
                }
            }
            catch (Exception exception) when (IsExpectedWmiFailure(exception))
            {
                Debug.WriteLine($"Applying laptop brightness failed: {exception.Message}");
            }
        }

        private static BrightnessRange ReadRange(ManagementBaseObject instance)
        {
            int current = Convert.ToInt32(instance["CurrentBrightness"]);
            int minimum = 0;
            int maximum = 100;

            if (instance["Level"] is byte[] { Length: > 0 } levels)
            {
                minimum = levels[0];
                maximum = levels[^1];
            }

            if (maximum <= minimum)
            {
                minimum = 0;
                maximum = 100;
            }

            return new BrightnessRange(minimum, maximum, Math.Clamp(current, minimum, maximum));
        }

        /// <summary>
        /// Machines without a WMI-controllable panel (most desktops) surface this as an
        /// exception rather than an empty result, so absence of the feature is not an error.
        /// </summary>
        private static bool IsExpectedWmiFailure(Exception exception) =>
            exception is ManagementException
                or UnauthorizedAccessException
                or COMException
                or InvalidCastException
                or NotSupportedException;
    }
}
