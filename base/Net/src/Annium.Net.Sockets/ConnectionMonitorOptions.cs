namespace Annium.Net.Sockets;

/// <summary>
/// Configuration options for connection monitoring behavior
/// </summary>
public record ConnectionMonitorOptions
{
    /// <summary>
    /// Gets or sets the factory for creating connection monitors. If null, no monitoring is performed
    /// </summary>
    public IConnectionMonitorFactory? Factory { get; init; }

    /// <summary>
    /// Gets or sets the interval between ping messages in milliseconds (default: 60000ms)
    /// </summary>
    public int PingInterval { get; init; } = 60_000;

    /// <summary>
    /// Gets or sets the maximum delay allowed for ping responses in milliseconds (default: 300000ms)
    /// </summary>
    public long MaxPingDelay { get; init; } = 300_000;
}
