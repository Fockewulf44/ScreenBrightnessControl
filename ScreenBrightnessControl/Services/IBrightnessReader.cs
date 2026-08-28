namespace ScreenBrightnessControl.Services
{
    /// <summary>
    /// Reads the brightness a display currently reports.
    /// Kept separate from <see cref="IBrightnessWriter"/> so that read-only callers
    /// (for example the start-up synchronisation) do not depend on the ability to write.
    /// </summary>
    public interface IBrightnessReader
    {
        /// <summary>
        /// Returns the current brightness and its bounds, or <c>null</c> when the display
        /// does not expose brightness control.
        /// </summary>
        BrightnessRange? TryReadRange();
    }
}
