namespace Annium.Net.Sockets.Tests;

/// <summary>
/// Transport the socket test fixtures run over.
/// </summary>
public enum StreamType
{
    /// <summary>Plain, unencrypted TCP stream.</summary>
    Plain,

    /// <summary>TLS-wrapped stream.</summary>
    Ssl,
}
