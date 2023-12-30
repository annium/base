namespace Annium.Net.WebSockets;

public record ServerWebSocketOptions
{
    public static ServerWebSocketOptions Default { get; } = new();
    public ConnectionMonitorOptions ConnectionMonitor { get; init; } = new();
}
