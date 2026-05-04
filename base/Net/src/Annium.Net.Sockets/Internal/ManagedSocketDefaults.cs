namespace Annium.Net.Sockets.Internal;

/// <summary>
/// Shared default values for managed-socket options. Defined once here and consumed by
/// <see cref="Annium.Net.Sockets.ClientSocketOptions"/>, <see cref="Annium.Net.Sockets.ServerSocketOptions"/>,
/// and <see cref="ManagedSocketOptionsBase"/> so a default change only needs to be made in one place.
/// </summary>
internal static class ManagedSocketDefaults
{
    /// <summary>
    /// Default buffer size for socket read/write operations, in bytes.
    /// </summary>
    public const int BufferSize = 65_536;

    /// <summary>
    /// Default upper bound on a single message size, in bytes; messages whose decoded length
    /// exceeds this value cause the receive loop to close the connection with an error.
    /// </summary>
    public const int ExtremeMessageSize = 1_048_576;
}
