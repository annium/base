using System;

namespace Annium.Logging.Shared;

public record LogRouteConfiguration
{
    public TimeSpan BufferTime { get; set; } = TimeSpan.FromMinutes(1);
    public int BufferCount { get; set; } = 100;
}