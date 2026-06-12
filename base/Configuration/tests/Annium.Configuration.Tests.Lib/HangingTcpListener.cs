using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Configuration.Tests.Lib;

/// <summary>
/// Local TCP listener that accepts connections but never sends a response — used to force
/// a deterministic <c>HttpClient</c> timeout trigger regardless of the test host's network
/// configuration. Holds strong references to accepted clients so GC can't reap them
/// mid-test (which would otherwise close the socket and translate timeout into IO error).
/// </summary>
public sealed class HangingTcpListener : TcpListenerBase
{
    /// <summary>Strong references to accepted clients, kept alive until cleanup to prevent premature socket disposal.</summary>
    private readonly List<TcpClient> _accepted = new();

    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="resourcePath">The resource path appended to the listener's URI (e.g. "config.json").</param>
    public HangingTcpListener(string resourcePath)
        : base(resourcePath) { }

    /// <summary>
    /// Accepts the client connection and stores a strong reference to it, intentionally
    /// not sending any data so the caller's HTTP request hangs until the client is disposed.
    /// </summary>
    /// <param name="client">The accepted TCP client to hold alive for the duration of the test.</param>
    /// <param name="ct">Cancellation token (not observed — hanging behaviour is intentional).</param>
    /// <returns>A completed value task; processing is synchronous (just an add to the list).</returns>
    protected override ValueTask HandleClientAsync(TcpClient client, CancellationToken ct)
    {
        // Hold a strong reference so GC doesn't reap the socket while the test
        // is mid-request — otherwise HttpClient sees an IO error, not a timeout.
        lock (_accepted)
            _accepted.Add(client);
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Disposes all accepted client sockets that were held alive during the test, releasing
    /// TCP resources after the listener has stopped accepting new connections.
    /// </summary>
    /// <returns>A task that completes when all held clients have been disposed.</returns>
    protected override async ValueTask CleanupAsync()
    {
        // Snapshot under the lock, then dispose outside it — DisposeAsync cannot be awaited
        // while holding the lock. The accept loop has already terminated by the time Cleanup
        // runs, so no concurrent Add can race the snapshot.
        TcpClient[] clients;
        lock (_accepted)
        {
            clients = _accepted.ToArray();
            _accepted.Clear();
        }
        foreach (var c in clients)
            await c.DisposeAsync();
    }
}
