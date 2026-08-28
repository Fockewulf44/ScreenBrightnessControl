using System.Collections.Generic;

namespace ScreenBrightnessControl.Interop
{
    /// <summary>
    /// Supplies the physical monitors currently attached to the machine.
    /// Abstracted so that brightness services depend on this contract rather than on user32/dxva2.
    /// </summary>
    internal interface IPhysicalMonitorProvider
    {
        /// <summary>
        /// Returns the attached physical monitors. The caller owns the returned handles and must dispose them.
        /// </summary>
        IReadOnlyList<PhysicalMonitorHandle> GetPhysicalMonitors();
    }
}
