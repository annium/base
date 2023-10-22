namespace Annium.Net.WebSockets;

public record ClientWebSocketOptions
{
    public static ClientWebSocketOptions Default { get; } = new();
    public IConnectionMonitor ConnectionMonitor { get; init; } = WebSockets.ConnectionMonitor.None;
    public int KeepAliveInterval { get; init; } = 20_000;
    public int ConnectTimeout { get; init; } = 3_000;
    public int ReconnectDelay { get; init; }
}