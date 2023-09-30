using System;

namespace Annium.Net.WebSockets.Internal;

internal readonly struct WebSocketCloseResult
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