using System;

namespace Annium.Net.Sockets.Internal;

internal readonly struct ReceiveResult
{
    public readonly int Count;
    public readonly SocketCloseStatus? Status;
    public readonly Exception? Exception;

    public ReceiveResult(
        int count,
        SocketCloseStatus? status,
        Exception? exception
    )
    {
        Count = count;
        Status = status;
        Exception = exception;
    }
}