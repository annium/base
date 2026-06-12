namespace Annium.Net.Sockets.Internal;

/// <summary>
/// Base options for managed socket configuration.
/// </summary>
internal record ManagedSocketOptionsBase
{
    /// <summary>
    /// Gets the default managed socket options.
    /// </summary>
    public static ManagedSocketOptionsBase Default { get; } = new();

    /// <summary>
    /// Gets or sets the buffer size for socket operations.
    /// </summary>
    public int BufferSize { get; init; } = ManagedSocketDefaults.BufferSize;

    /// <summary>
    /// Gets or sets the maximum size for extremely large messages.
    /// </summary>
    public int ExtremeMessageSize { get; init; } = ManagedSocketDefaults.ExtremeMessageSize;
}
