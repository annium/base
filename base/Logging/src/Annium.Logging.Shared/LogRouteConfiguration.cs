using System;

namespace Annium.Logging.Shared;

/// <summary>
/// Configuration settings for log route buffering behavior
/// </summary>
public record LogRouteConfiguration
{
    /// <summary>
    /// Gets or sets the maximum time to buffer log messages before flushing
    /// </summary>
    public TimeSpan BufferTime { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Gets or sets the maximum number of log messages to buffer before flushing
    /// </summary>
    public int BufferCount { get; set; } = 5;
}
