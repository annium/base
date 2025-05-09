using System;
using System.Reactive.Linq;

namespace Annium.Net.Sockets;

public static class ReceivingSocketExtensions
{
    public static IObservable<ReadOnlyMemory<byte>> Observe(this IReceivingSocket socket) =>
        Observable.FromEvent<ReadOnlyMemory<byte>>(x => socket.OnReceived += x, x => socket.OnReceived -= x);
}
