namespace Annium.Net.Sockets;

public record ServerSocketOptions
{
    public static ServerSocketOptions Default { get; } = new();
    public SocketMode Mode { get; init; } = SocketMode.Raw;
    public IConnectionMonitorFactory? ConnectionMonitorFactory { get; init; }
    public int BufferSize { get; init; } = 65_536;
    public int ExtremeMessageSize { get; init; } = 1_048_576;
}
