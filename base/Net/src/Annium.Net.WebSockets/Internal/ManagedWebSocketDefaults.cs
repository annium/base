namespace Annium.Net.WebSockets.Internal;

/// <summary>
/// Shared default values for the managed-WebSocket internals. Defined once here so a default
/// change only needs to be made in one place (mirrors the TCP sibling's <c>ManagedSocketDefaults</c>).
/// </summary>
internal static class ManagedWebSocketDefaults
{
    /// <summary>
    /// Default buffer size for WebSocket message receiving operations, in bytes.
    /// </summary>
    public const int BufferSize = 65_536;
}
