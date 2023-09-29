namespace Annium.Net.WebSockets;

public record ClientWebSocketOptions
{
    public static ClientWebSocketOptions Default { get; } = new();
    public IConnectionMonitor ConnectionMonitor { get; init; } = WebSockets.ConnectionMonitor.None;
    public int ReconnectDelay { get; init; }
}