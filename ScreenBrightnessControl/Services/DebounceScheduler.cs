using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace ScreenBrightnessControl.Services
{
    /// <summary>
    /// Debounces work by a fixed delay. Every new request cancels the pending one, so the
    /// action runs once, <see cref="_delay"/> after the last request.
    /// </summary>
    /// <remarks>
    /// Instances are affine to the thread that schedules on them (the UI thread here) and are
    /// not safe for concurrent use from several threads.
    /// </remarks>
    public sealed class DebounceScheduler : IDebounceScheduler
    {
        private readonly TimeSpan _delay;
        private CancellationTokenSource? _pending;
        private bool _disposed;

        public DebounceScheduler(TimeSpan delay)
        {
            if (delay < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(delay), delay, "The delay cannot be negative.");
            }

            _delay = delay;
        }

        public void Schedule(Func<CancellationToken, Task> action)
        {
            ArgumentNullException.ThrowIfNull(action);
            ObjectDisposedException.ThrowIf(_disposed, this);

            Cancel();

            CancellationTokenSource source = new();
            _pending = source;

            _ = RunAsync(action, source);
        }

        public void Cancel()
        {
            // Disposal is left to the run that owns the source, so that a continuation which has
            // been signalled but has not resumed yet never observes a disposed token.
            _pending?.Cancel();
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            Cancel();
        }

        private async Task RunAsync(Func<CancellationToken, Task> action, CancellationTokenSource source)
        {
            CancellationToken token = source.Token;

            try
            {
                // ConfigureAwait(true) keeps the continuation on the scheduling (UI) context,
                // so the action itself decides what to push onto a background thread.
                await Task.Delay(_delay, token).ConfigureAwait(true);
                await action(token).ConfigureAwait(true);
            }
            catch (OperationCanceledException)
            {
                // A newer request superseded this one; nothing to do.
            }
            catch (Exception exception)
            {
                // Nothing awaits this task, so a failure must not become an unobserved exception.
                Debug.WriteLine($"A debounced action failed: {exception}");
            }
            finally
            {
                if (ReferenceEquals(_pending, source))
                {
                    _pending = null;
                }

                source.Dispose();
            }
        }
    }
}
