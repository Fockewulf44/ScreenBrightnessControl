using System;
using System.Threading;
using System.Threading.Tasks;

namespace ScreenBrightnessControl.Services
{
    /// <summary>
    /// Delays work until the caller has stopped requesting it, so that a burst of
    /// requests (a slider being dragged) results in a single execution.
    /// </summary>
    public interface IDebounceScheduler : IDisposable
    {
        /// <summary>
        /// Cancels any execution still waiting and schedules <paramref name="action"/> instead.
        /// </summary>
        void Schedule(Func<CancellationToken, Task> action);

        /// <summary>
        /// Cancels the execution still waiting, if any.
        /// </summary>
        void Cancel();
    }
}
