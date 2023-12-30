namespace Annium.Net.WebSockets;

public record ClientWebSocketOptions
{
    public static ClientWebSocketOptions Default { get; } = new();
    public ConnectionMonitorOptions ConnectionMonitor { get; init; } = new();
    public int KeepAliveInterval { get; init; } = 3_000;
    public int ConnectTimeout { get; init; } = 5_000;
    public int ReconnectDelay { get; init; }
}
