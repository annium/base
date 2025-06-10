namespace Annium.Net.Sockets.Internal;

/// <summary>
/// Base options for managed socket configuration
/// </summary>
internal record ManagedSocketOptionsBase
{
    /// <summary>
    /// Gets the default managed socket options
    /// </summary>
    public static ManagedSocketOptionsBase Default { get; } = new();

    /// <summary>
    /// Gets or sets the buffer size for socket operations (default: 65536 bytes)
    /// </summary>
    public int BufferSize { get; init; } = 65_536;

    /// <summary>
    /// Gets or sets the maximum size for extremely large messages (default: 1048576 bytes)
    /// </summary>
    public int ExtremeMessageSize { get; init; } = 1_048_576;
}
