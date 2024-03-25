using System;

namespace Annium.Logging.Shared;

public record LogRouteConfiguration
{
    public TimeSpan BufferTime { get; set; } = TimeSpan.FromSeconds(5);
    public int BufferCount { get; set; } = 5;
}
