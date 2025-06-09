namespace Annium.Net.WebSockets;

/// <summary>
/// Configuration options for client WebSocket connections
/// </summary>
public record ClientWebSocketOptions
{
    /// <summary>
    /// Gets the default configuration options for client WebSocket connections
    /// </summary>
    public static ClientWebSocketOptions Default { get; } = new();

    /// <summary>
    /// Gets or sets the connection monitor configuration options
    /// </summary>
    public ConnectionMonitorOptions ConnectionMonitor { get; init; } = new();

    /// <summary>
    /// Gets or sets the keep-alive interval in milliseconds (default: 3000ms)
    /// </summary>
    public int KeepAliveInterval { get; init; } = 3_000;

    /// <summary>
    /// Gets or sets the connection timeout in milliseconds (default: 5000ms)
    /// </summary>
    public int ConnectTimeout { get; init; } = 5_000;

    /// <summary>
    /// Gets or sets the delay before attempting to reconnect in milliseconds (default: 0ms)
    /// </summary>
    public int ReconnectDelay { get; init; }
}
