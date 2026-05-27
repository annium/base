using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Net.Sockets.Internal;
using Annium.Testing;
using Xunit;

namespace Annium.Net.Sockets.Tests.Internal;

/// <summary>
/// Regression test for the semaphore-release-without-acquire bug in
/// <see cref="MessagingManagedSocket.SendAsync"/>. If <c>WaitAsync</c> throws
/// <see cref="System.OperationCanceledException"/> before acquiring the gate, the
/// <c>finally</c> block must not call <c>Release()</c>; otherwise the semaphore count
/// exceeds its maximum and every subsequent send throws
/// <see cref="System.Threading.SemaphoreFullException"/>.
/// </summary>
public class MessagingManagedSocketSemaphoreGuardTests
{
    /// <summary>
    /// A pre-cancelled cancellation token passed to <c>SendAsync</c> must NOT corrupt the
    /// internal semaphore. The call returns <see cref="SocketSendStatus.Canceled"/> and a
    /// subsequent send with a live token succeeds.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task SendAsync_PreCancelledToken_DoesNotCorruptSemaphore()
    {
        // arrange — a MemoryStream is enough; we exercise the semaphore path only
        var stream = new MemoryStream();
        var socket = new MessagingManagedSocket(stream, ManagedSocketOptionsBase.Default, VoidLogger.Instance);

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        var payload = new byte[] { 1, 2, 3 };

        // act — pre-cancelled send must not throw (the fix suppresses the spurious Release)
        var cancelled = await socket.SendAsync(payload, cts.Token);

        // assert — returns Canceled (not a propagated SemaphoreFullException)
        cancelled.Is(SocketSendStatus.Canceled);

        // act — subsequent send with a live token must succeed; semaphore is intact
        var ok = await socket.SendAsync(payload, CancellationToken.None);

        // assert — would throw SemaphoreFullException under the old buggy Release pattern
        ok.Is(SocketSendStatus.Ok);
    }
}
