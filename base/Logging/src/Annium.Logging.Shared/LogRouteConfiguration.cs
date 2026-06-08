using System;

namespace Annium.Logging.Shared;

/// <summary>
/// Configuration settings for log route buffering behavior
/// </summary>
public record LogRouteConfiguration
{
    /// <summary>
    /// Gets the maximum time to buffer log messages before flushing
    /// </summary>
    public TimeSpan BufferTime { get; init; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Gets the maximum number of log messages to buffer before flushing
    /// </summary>
    public int BufferCount { get; init; } = 5;
}
