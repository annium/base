using System;
using System.Reactive.Linq;

namespace Annium.Net.WebSockets;

public static class ReceivingWebSocketExtensions
{
    public static IObservable<ReadOnlyMemory<byte>> ObserveText(this IReceivingWebSocket socket) =>
        Observable.FromEvent<ReadOnlyMemory<byte>>(
            x => socket.TextReceived += x,
            x => socket.TextReceived -= x
        );

    public static IObservable<ReadOnlyMemory<byte>> ObserveBinary(this IReceivingWebSocket socket) =>
        Observable.FromEvent<ReadOnlyMemory<byte>>(
            x => socket.BinaryReceived += x,
            x => socket.BinaryReceived -= x
        );
}