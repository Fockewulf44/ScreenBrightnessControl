namespace ScreenBrightnessControl.Services
{
    /// <summary>
    /// A display whose brightness can be both read and written.
    /// </summary>
    public interface IBrightnessController : IBrightnessReader, IBrightnessWriter
    {
        /// <summary>
        /// Human readable name of the display this controller drives.
        /// </summary>
        string DisplayName { get; }
    }
}
