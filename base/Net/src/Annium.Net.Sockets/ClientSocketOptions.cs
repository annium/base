namespace Annium.Net.Sockets;

public record ClientSocketOptions
{
    public static ClientSocketOptions Default { get; } = new();
    public SocketMode Mode { get; init; } = SocketMode.Raw;
    public IConnectionMonitor ConnectionMonitor { get; init; } = Sockets.ConnectionMonitor.None;
    public int ConnectTimeout { get; init; } = 3_000;
    public int ReconnectDelay { get; init; }
}