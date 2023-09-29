namespace Annium.Net.Sockets;

public record ClientSocketOptions
{
    public static ClientSocketOptions Default { get; } = new();
    public IConnectionMonitor ConnectionMonitor { get; init; } = Sockets.ConnectionMonitor.None;
    public int ReconnectDelay { get; init; }
}