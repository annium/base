namespace Annium.Net.Sockets;

public record ConnectionMonitorOptions
{
    public IConnectionMonitorFactory? Factory { get; init; }
    public int PingInterval { get; init; } = 60_000;
    public long MaxPingDelay { get; init; } = 300_000;
}
