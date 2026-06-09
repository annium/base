using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;

namespace Annium.Net.Sockets.Internal;

/// <summary>
/// Shared helpers for the public socket extension methods to avoid duplicating subscription/teardown
/// scaffolding between client and server variants.
/// </summary>
internal static class SocketEventHelpers
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
    public static async Task<SocketCloseStatus> WaitForDisconnectAsync(
        Action<Action<SocketCloseStatus>> subscribe,
        Action<Action<SocketCloseStatus>> unsubscribe,
        ILogSubject log,
        CancellationToken ct
    )
    {
        var tcs = new TaskCompletionSource<SocketCloseStatus>(TaskCreationOptions.RunContinuationsAsynchronously);

        log.Trace<string>("subscribe {tcs} to OnDisconnected", tcs.GetFullId());

        Action<SocketCloseStatus> handler = status =>
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
