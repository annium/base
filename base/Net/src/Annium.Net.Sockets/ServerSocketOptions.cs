namespace Annium.Net.Sockets;

/// <summary>
/// Configuration options for server socket behavior
/// </summary>
public record ServerSocketOptions
{
    /// <summary>
    /// Gets the default server socket options
    /// </summary>
    public static ServerSocketOptions Default { get; } = new();

    /// <summary>
    /// Gets or sets the socket operating mode (Raw or Messaging)
    /// </summary>
    public SocketMode Mode { get; init; } = SocketMode.Raw;

    /// <summary>
    /// Gets or sets the connection monitor options for detecting connection health
    /// </summary>
    public ConnectionMonitorOptions ConnectionMonitor { get; init; } = new();

    /// <summary>
    /// Gets or sets the buffer size for socket operations (default: 65536 bytes)
    /// </summary>
    public int BufferSize { get; init; } = 65_536;

    /// <summary>
    /// Gets or sets the maximum size for extremely large messages (default: 1048576 bytes)
    /// </summary>
    public int ExtremeMessageSize { get; init; } = 1_048_576;
}
