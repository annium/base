namespace Annium.Net.WebSockets;

/// <summary>
/// Configuration options for WebSocket connection monitoring
/// </summary>
public record ConnectionMonitorOptions
{
    /// <summary>
    /// Gets or sets the factory for creating connection monitors (null disables monitoring)
    /// </summary>
    public IConnectionMonitorFactory? Factory { get; init; }

    /// <summary>
    /// Gets or sets the interval between ping messages in milliseconds (default: 60000ms)
    /// </summary>
    public int PingInterval { get; init; } = 60_000;

    /// <summary>
    /// Gets or sets the maximum allowed ping delay in milliseconds (default: 300000ms)
    /// </summary>
    public long MaxPingDelay { get; init; } = 300_000;
}
