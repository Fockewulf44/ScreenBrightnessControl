using System;

namespace ScreenBrightnessControl.Services
{
    /// <summary>
    /// The brightness a display currently reports, together with the bounds it accepts.
    /// </summary>
    public readonly record struct BrightnessRange(int Minimum, int Maximum, int Current)
    {
        /// <summary>
        /// Constrains a requested value to the bounds this display reported.
        /// </summary>
        public int Clamp(int value) => Math.Clamp(value, Minimum, Maximum);
    }
}
