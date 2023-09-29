namespace Annium.Net.Sockets;

public record ServerSocketOptions
{
    public static ServerSocketOptions Default { get; } = new();
    public IConnectionMonitor ConnectionMonitor { get; init; } = Sockets.ConnectionMonitor.None;
}