namespace Annium.Threading;

/// <summary>
/// Shared default poll cadence for the area's polling primitives (<see cref="Tasks.Wait"/> and
/// <see cref="Channels.ChannelReaderExtensions"/>), keeping the value defined in one place.
/// </summary>
internal static class PollingDefaults
{
    /// <summary>
    /// Default poll cadence in milliseconds.
    /// </summary>
    internal const int PollDelayMs = 25;
}
