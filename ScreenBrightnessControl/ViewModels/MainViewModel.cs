using System;
using System.Threading.Tasks;

namespace ScreenBrightnessControl.ViewModels
{
    /// <summary>
    /// Aggregates the sliders shown by the main window. It composes existing view models
    /// rather than knowing how any particular display is driven.
    /// </summary>
    public sealed class MainViewModel : IDisposable
    {
        private bool _disposed;

        public MainViewModel(BrightnessSliderViewModel laptopScreen, BrightnessSliderViewModel externalMonitor)
        {
            LaptopScreen = laptopScreen ?? throw new ArgumentNullException(nameof(laptopScreen));
            ExternalMonitor = externalMonitor ?? throw new ArgumentNullException(nameof(externalMonitor));
        }

        public BrightnessSliderViewModel LaptopScreen { get; }

        public BrightnessSliderViewModel ExternalMonitor { get; }

        /// <summary>
        /// Populates every slider with the brightness its display currently reports.
        /// </summary>
        public async Task LoadAsync()
        {
            await LaptopScreen.LoadAsync().ConfigureAwait(true);
            await ExternalMonitor.LoadAsync().ConfigureAwait(true);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            LaptopScreen.Dispose();
            ExternalMonitor.Dispose();
        }
    }
}
