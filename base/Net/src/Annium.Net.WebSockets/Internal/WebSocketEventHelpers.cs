using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;

namespace Annium.Net.WebSockets.Internal;

/// <summary>
/// Shared helpers for the public WebSocket extension methods to avoid duplicating
/// subscription/teardown scaffolding between client and server variants. Mirror of the TCP
/// sibling's <c>SocketEventHelpers</c>.
/// </summary>
internal static class WebSocketEventHelpers
{
    /// <summary>
    /// Subscribes a one-shot handler that completes the returned task with the close status the first
    /// time the underlying <c>OnDisconnected</c> event fires, then unsubscribes.
    /// </summary>
    /// <param name="subscribe">Callback that wires up an event handler.</param>
    /// <param name="unsubscribe">Callback that tears the same handler down.</param>
    /// <param name="log">Log subject for tracing.</param>
    /// <param name="ct">Cancellation token applied to the wait.</param>
    /// <returns>The disconnect status reported by the first event invocation.</returns>
    public static async Task<WebSocketCloseStatus> WaitForDisconnectAsync(
        Action<Action<WebSocketCloseStatus>> subscribe,
        Action<Action<WebSocketCloseStatus>> unsubscribe,
        ILogSubject log,
        CancellationToken ct
    )
    {
        var tcs = new TaskCompletionSource<WebSocketCloseStatus>(TaskCreationOptions.RunContinuationsAsynchronously);

        log.Trace<string>("subscribe {tcs} to OnDisconnected", tcs.GetFullId());

        Action<WebSocketCloseStatus> handler = status =>
        {
            log.Trace<string>("set {tcs} to signaled state", tcs.GetFullId());
            tcs.TrySetResult(status);
        };

        subscribe(handler);

        // unsubscribe in finally so a cancelled wait does not leak the handler (the handler no
        // longer self-unsubscribes).
        try
        {
            return await tcs.Task.WaitAsync(ct);
        }
        finally
        {
            unsubscribe(handler);
        }
    }
}
