using System;

namespace Annium.Net.Sockets;

public readonly struct SocketCloseResult
{
    public readonly SocketCloseStatus Status;
    public readonly Exception? Exception;

    public SocketCloseResult(
        SocketCloseStatus status,
        Exception? exception
    )
    {
        Status = status;
        Exception = exception;
    }
}