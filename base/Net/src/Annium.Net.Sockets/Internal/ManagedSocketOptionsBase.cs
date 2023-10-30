namespace Annium.Net.Sockets.Internal;

internal record ManagedSocketOptionsBase
{
    public static ManagedSocketOptionsBase Default { get; } = new();
    public int BufferSize { get; init; } = 65_536;
    public int ExtremeMessageSize { get; init; } = 1_048_576;
}
