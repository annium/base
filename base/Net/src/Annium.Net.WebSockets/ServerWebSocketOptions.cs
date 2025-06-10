namespace Annium.Net.WebSockets;

/// <summary>
/// Configuration options for server WebSocket connections
/// </summary>
public record ServerWebSocketOptions
{
    /// <summary>
    /// Gets the default configuration options for server WebSocket connections
    /// </summary>
    public static ServerWebSocketOptions Default { get; } = new();

    /// <summary>
    /// Gets or sets the connection monitor configuration options
    /// </summary>
    public ConnectionMonitorOptions ConnectionMonitor { get; init; } = new();
}
