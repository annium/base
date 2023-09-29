using System;

namespace Annium.Net.WebSockets;

public readonly struct WebSocketCloseResult
{
    public readonly WebSocketCloseStatus Status;
    public readonly Exception? Exception;

    public WebSocketCloseResult(
        WebSocketCloseStatus status,
        Exception? exception
    )
    {
        Status = status;
        Exception = exception;
    }
}