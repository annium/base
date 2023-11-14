namespace Annium.Net.Sockets;

public record ClientSocketOptions
{
    public static ClientSocketOptions Default { get; } = new();
    public SocketMode Mode { get; init; } = SocketMode.Raw;
    public ConnectionMonitorOptions ConnectionMonitor { get; init; } = new();
    public int ConnectTimeout { get; init; } = 5_000;
    public int ReconnectDelay { get; init; }
    public int BufferSize { get; init; } = 65_536;
    public int ExtremeMessageSize { get; init; } = 1_048_576;
}
