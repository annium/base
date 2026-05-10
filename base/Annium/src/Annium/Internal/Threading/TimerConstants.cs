using System;

namespace Annium.Internal.Threading;

/// <summary>
/// Shared constants for the timer base classes. Extracted so that <see cref="AsyncTimerBase"/>,
/// <see cref="SyncTimerBase"/>, and <see cref="DebounceTimerBase"/> all reference the same
/// dispose-budget value rather than triplicating it.
/// </summary>
internal static class TimerConstants
{
    /// <summary>
    /// Maximum time a timer base waits during <c>Dispose</c> for the underlying <see cref="System.Threading.Timer"/>
    /// drain (and, where applicable, the in-flight callback gate) before leaking the wait handle / gate and
    /// logging a warning. A bounded budget keeps disposal predictable while still giving short-running
    /// callbacks time to complete.
    /// </summary>
    internal static readonly TimeSpan DisposeWaitBudget = TimeSpan.FromSeconds(5);
}
