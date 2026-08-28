using System;

namespace ScreenBrightnessControl.Interop
{
    /// <summary>
    /// Owns the lifetime of a single DDC/CI physical monitor handle so that callers
    /// never have to remember to call DestroyPhysicalMonitor themselves.
    /// </summary>
    internal sealed class PhysicalMonitorHandle : IDisposable
    {
        private bool _disposed;

        internal PhysicalMonitorHandle(IntPtr handle, string description)
        {
            Handle = handle;
            Description = description;
        }

        internal IntPtr Handle { get; }

        internal string Description { get; }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            if (Handle != IntPtr.Zero)
            {
                NativeMethods.DestroyPhysicalMonitor(Handle);
            }
        }
    }
}
