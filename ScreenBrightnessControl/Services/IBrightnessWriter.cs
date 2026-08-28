namespace ScreenBrightnessControl.Services
{
    /// <summary>
    /// Applies a brightness value to a display.
    /// </summary>
    public interface IBrightnessWriter
    {
        /// <summary>
        /// Applies the requested brightness, clamped to whatever the display accepts.
        /// Implementations must not throw when the display refuses the request.
        /// </summary>
        void SetBrightness(int brightness);
    }
}
