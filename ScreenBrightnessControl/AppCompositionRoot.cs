using System;
using ScreenBrightnessControl.Services;
using ScreenBrightnessControl.ViewModels;

namespace ScreenBrightnessControl
{
    /// <summary>
    /// The single place that knows which concrete implementations the application uses.
    /// Everything else depends on abstractions only, so swapping an implementation
    /// (or a delay) is a change here and nowhere else.
    /// </summary>
    internal static class AppCompositionRoot
    {
        /// <summary>
        /// How long the application waits after the last slider movement before talking to the
        /// display. Applying on every intermediate value is expensive, so the writes are debounced.
        /// </summary>
        private static readonly TimeSpan BrightnessApplyDelay = TimeSpan.FromSeconds(0.5);

        internal static MainViewModel CreateMainViewModel()
        {
            BrightnessSliderViewModel laptopScreen = CreateSlider(new LaptopBrightnessController());
            BrightnessSliderViewModel externalMonitor = CreateSlider(new ExternalMonitorBrightnessController());

            return new MainViewModel(laptopScreen, externalMonitor);
        }

        /// <summary>
        /// Each slider gets its own scheduler so that moving one does not postpone the other.
        /// </summary>
        private static BrightnessSliderViewModel CreateSlider(IBrightnessController controller) =>
            new(controller, new DebounceScheduler(BrightnessApplyDelay));
    }
}
