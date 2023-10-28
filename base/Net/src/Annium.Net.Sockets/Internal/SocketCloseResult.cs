using System;

namespace Annium.Net.Sockets.Internal;

internal readonly struct SocketCloseResult
{
    public readonly SocketCloseStatus Status;
    public readonly Exception? Exception;

    public SocketCloseResult(SocketCloseStatus status, Exception? exception)
    {
        Status = status;
        Exception = exception;
    }
}
