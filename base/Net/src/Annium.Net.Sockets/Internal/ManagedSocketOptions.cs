namespace Annium.Net.Sockets.Internal;

internal sealed record ManagedSocketOptions : ManagedSocketOptionsBase
{
    public static ManagedSocketOptions Default { get; } = new();
    public SocketMode Mode { get; init; } = SocketMode.Raw;
}
