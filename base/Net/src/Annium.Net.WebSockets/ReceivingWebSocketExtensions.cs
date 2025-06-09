using System;
using System.Reactive.Linq;

namespace Annium.Net.WebSockets;

/// <summary>
/// Extension methods for receiving WebSocket operations using reactive extensions
/// </summary>
public static class ReceivingWebSocketExtensions
{
    /// <summary>
    /// Creates an observable sequence for text messages received on the WebSocket
    /// </summary>
    /// <param name="socket">The receiving WebSocket to observe</param>
    /// <returns>An observable sequence of text message data</returns>
    public static IObservable<ReadOnlyMemory<byte>> ObserveText(this IReceivingWebSocket socket) =>
        Observable.FromEvent<ReadOnlyMemory<byte>>(x => socket.OnTextReceived += x, x => socket.OnTextReceived -= x);

    /// <summary>
    /// Creates an observable sequence for binary messages received on the WebSocket
    /// </summary>
    /// <param name="socket">The receiving WebSocket to observe</param>
    /// <returns>An observable sequence of binary message data</returns>
    public static IObservable<ReadOnlyMemory<byte>> ObserveBinary(this IReceivingWebSocket socket) =>
        Observable.FromEvent<ReadOnlyMemory<byte>>(
            x => socket.OnBinaryReceived += x,
            x => socket.OnBinaryReceived -= x
        );
}
