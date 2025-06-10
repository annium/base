namespace Annium.Net.Sockets.Internal;

/// <summary>
/// Configuration options for managed socket operations
/// </summary>
internal sealed record ManagedSocketOptions : ManagedSocketOptionsBase
{
    /// <summary>
    /// Gets the default managed socket options
    /// </summary>
    public static new ManagedSocketOptions Default { get; } = new();

    /// <summary>
    /// Gets or sets the socket operating mode (default: Raw)
    /// </summary>
    public SocketMode Mode { get; init; } = SocketMode.Raw;
}
