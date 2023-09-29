namespace Annium.Net.WebSockets;

public record ServerWebSocketOptions
{
    public static ServerWebSocketOptions Default { get; } = new();
    public IConnectionMonitor ConnectionMonitor { get; init; } = WebSockets.ConnectionMonitor.None;
}