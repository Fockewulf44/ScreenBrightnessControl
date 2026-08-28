using System;
using System.Threading;
using System.Threading.Tasks;
using ScreenBrightnessControl.Services;

namespace ScreenBrightnessControl.ViewModels
{
    /// <summary>
    /// Backs a single brightness slider: it publishes the value the display actually reports and
    /// writes user changes back once the user has stopped dragging.
    /// </summary>
    public sealed class BrightnessSliderViewModel : ObservableObject, IDisposable
    {
        private readonly IBrightnessController _controller;
        private readonly IDebounceScheduler _scheduler;

        private double _value;
        private double _minimum;
        private double _maximum = 100;
        private bool _isAvailable;

        /// <summary>
        /// Guards against writing brightness back while the view model is adopting the value
        /// it just read from the display.
        /// </summary>
        private bool _isSynchronising;

        /// <summary>
        /// Bindings can push a final value while the window is tearing down, which must not
        /// schedule work on an already disposed scheduler.
        /// </summary>

        private bool _disposed;

        public BrightnessSliderViewModel(IBrightnessController controller, IDebounceScheduler scheduler)
        {
            _controller = controller ?? throw new ArgumentNullException(nameof(controller));
            _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
        }

        /// <summary>
        /// Label shown above the slider.
        /// </summary>
        public string Header => _controller.DisplayName;

        /// <summary>
        /// <c>false</c> when the display does not expose brightness control, which keeps the
        /// slider disabled instead of letting the user move a control that does nothing.
        /// </summary>
        public bool IsAvailable
        {
            get => _isAvailable;
            private set => SetProperty(ref _isAvailable, value);
        }

        public double Minimum
        {
            get => _minimum;
            private set => SetProperty(ref _minimum, value);
        }

        public double Maximum
        {
            get => _maximum;
            private set => SetProperty(ref _maximum, value);
        }

        /// <summary>
        /// The slider position. Changes made by the user are applied to the display after the
        /// scheduler's delay has elapsed; changes made by <see cref="LoadAsync"/> are not applied back.
        /// </summary>
        public double Value
        {
            get => _value;
            set
            {
                if (!SetProperty(ref _value, value) || _isSynchronising || _disposed)
                {
                    return;
                }

                _scheduler.Schedule(ApplyAsync);
            }
        }

        /// <summary>
        /// Reads the display's real brightness and adopts it, without writing anything back.
        /// </summary>
        public async Task LoadAsync()
        {
            BrightnessRange? range = await Task.Run(_controller.TryReadRange).ConfigureAwait(true);

            _isSynchronising = true;

            try
            {
                IsAvailable = range.HasValue;

                if (!range.HasValue)
                {
                    return;
                }

                BrightnessRange actual = range.Value;

                // Bounds are published before the value so that the slider does not clamp
                // the incoming value against a stale range.
                Minimum = actual.Minimum;
                Maximum = actual.Maximum;
                Value = actual.Current;
            }
            finally
            {
                _isSynchronising = false;
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _scheduler.Dispose();
        }

        private Task ApplyAsync(CancellationToken cancellationToken)
        {
            int target = (int)Math.Round(_value);

            // The native calls block, so they are kept off the UI thread.
            return Task.Run(() => _controller.SetBrightness(target), cancellationToken);
        }
    }
}
