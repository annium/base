using System;
using System.Reactive.Linq;

namespace Annium.Net.Sockets;

/// <summary>
/// Extension methods for receiving socket operations
/// </summary>
public static class ReceivingSocketExtensions
{
    /// <summary>
    /// Creates an observable stream of received data from the socket
    /// </summary>
    /// <param name="socket">The receiving socket to observe</param>
    /// <returns>An observable stream of received binary data</returns>
    public static IObservable<ReadOnlyMemory<byte>> Observe(this IReceivingSocket socket) =>
        Observable.FromEvent<ReadOnlyMemory<byte>>(x => socket.OnReceived += x, x => socket.OnReceived -= x);
}
