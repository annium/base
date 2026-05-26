using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Configuration.Tests.Lib;

/// <summary>
/// Base for local loopback TCP listeners used in tests. Owns the listener socket, the
/// accept-loop lifecycle, and deterministic teardown; subclasses supply only the
/// per-connection handling via <see cref="HandleClientAsync"/> and optional cleanup via
/// <see cref="CleanupAsync"/>.
/// </summary>
public abstract class TcpListenerBase : IAsyncDisposable
{
    private readonly TcpListener _listener;
    private readonly TaskCompletionSource _listening = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly string _resourcePath;
    private Task? _acceptLoop;

    /// <summary>
    /// Cancellation source signalled on dispose. Subclasses observe the token passed to
    /// <see cref="HandleClientAsync"/>; it is cancelled before the accept loop is torn down.
    /// </summary>
    protected CancellationTokenSource Cts { get; } = new();

    /// <summary>
    /// Initializes a new instance bound to an OS-assigned loopback port.
    /// </summary>
    /// <param name="resourcePath">The resource path appended to the listener's URI (e.g. "config.json").</param>
    protected TcpListenerBase(string resourcePath)
    {
        _resourcePath = resourcePath;
        _listener = new TcpListener(IPAddress.Loopback, 0);
    }

    /// <summary>
    /// The loopback URI exposed by this listener.
    /// </summary>
    public Uri Uri => new($"http://127.0.0.1:{((IPEndPoint)_listener.LocalEndpoint).Port}/{_resourcePath}");

    /// <summary>
    /// Starts the listener and awaits the ready signal. The accept loop runs as a
    /// tracked <see cref="Task"/> stored in a private field so <see cref="DisposeAsync"/>
    /// can await its completion — propagating any unexpected exception instead of swallowing it.
    /// </summary>
    public async Task StartAsync(CancellationToken ct)
    {
        _listener.Start();
        _acceptLoop = Task.Run(async () =>
        {
            _listening.TrySetResult();
            try
            {
                while (!Cts.IsCancellationRequested)
                {
                    var client = await _listener.AcceptTcpClientAsync(Cts.Token);
                    await HandleClientAsync(client, Cts.Token);
                }
            }
            catch (OperationCanceledException)
            { /* expected on dispose */
            }
            catch (ObjectDisposedException)
            { /* expected on dispose */
            }
            catch (IOException)
            { /* client disconnected */
            }
        });

        await _listening.Task.WaitAsync(ct);
    }

    /// <summary>
    /// Handles a single accepted connection. Implementations own the lifetime of
    /// <paramref name="client"/> — dispose it when done, or retain it deliberately.
    /// </summary>
    /// <param name="client">The accepted TCP client.</param>
    /// <param name="ct">Token signalled when the listener is disposed.</param>
    protected abstract ValueTask HandleClientAsync(TcpClient client, CancellationToken ct);

    /// <summary>
    /// Releases subclass-owned resources after the accept loop has terminated. Called once
    /// during <see cref="DisposeAsync"/>; the base implementation is a no-op.
    /// </summary>
    protected virtual ValueTask CleanupAsync() => ValueTask.CompletedTask;

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await Cts.CancelAsync();
        _listener.Stop();
        if (_acceptLoop is not null)
        {
            try
            {
                // VSTHRD003: the accept-loop Task is started in StartAsync above (same instance, same context)
                // and Cancel + Stop guarantee its termination before this await — safe to await directly.
#pragma warning disable VSTHRD003
                await _acceptLoop;
#pragma warning restore VSTHRD003
            }
            catch (OperationCanceledException)
            { /* expected on cancel */
            }
            catch (ObjectDisposedException)
            { /* expected on listener.Stop */
            }
            catch (IOException)
            { /* client disconnected during dispose */
            }
        }
        await CleanupAsync();
        Cts.Dispose();
    }
}
