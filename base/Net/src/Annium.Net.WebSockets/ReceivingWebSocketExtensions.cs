using System;
using System.Reactive.Linq;

namespace Annium.Net.WebSockets;

public static class ReceivingWebSocketExtensions
{
    public static IObservable<ReadOnlyMemory<byte>> ObserveText(this IReceivingWebSocket socket) =>
        Observable.FromEvent<ReadOnlyMemory<byte>>(x => socket.OnTextReceived += x, x => socket.OnTextReceived -= x);

    public static IObservable<ReadOnlyMemory<byte>> ObserveBinary(this IReceivingWebSocket socket) =>
        Observable.FromEvent<ReadOnlyMemory<byte>>(
            x => socket.OnBinaryReceived += x,
            x => socket.OnBinaryReceived -= x
        );
}
