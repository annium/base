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
    /// <summary>The underlying TCP listener bound to the loopback interface.</summary>
    private readonly TcpListener _listener;

    /// <summary>Completion source resolved when the accept loop is ready to accept connections.</summary>
    private readonly TaskCompletionSource _listening = new(TaskCreationOptions.RunContinuationsAsynchronously);

    /// <summary>The resource path segment appended to the listener URI (e.g. "config.json").</summary>
    private readonly string _resourcePath;

    /// <summary>The background accept loop started by <see cref="StartAsync"/>; awaited during disposal.</summary>
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
    /// <param name="ct">Token used to bound the wait for the accept loop to signal readiness.</param>
    /// <returns>A task that completes once the accept loop has signalled it is ready.</returns>
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
            catch (SocketException) when (Cts.IsCancellationRequested)
            {
                // Aborting a pending AcceptTcpClientAsync via Stop() surfaces a platform-specific
                // failure: ObjectDisposedException on Windows/macOS, but SocketException
                // (EINVAL, "Invalid argument") on Linux. Only expected once teardown has begun —
                // an accept failure before that is a real fault and still propagates.
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
    /// <returns>A value task that completes when the connection has been fully handled.</returns>
    protected abstract ValueTask HandleClientAsync(TcpClient client, CancellationToken ct);

    /// <summary>
    /// Releases subclass-owned resources after the accept loop has terminated. Called once
    /// during <see cref="DisposeAsync"/>; the base implementation is a no-op.
    /// </summary>
    /// <returns>A value task that completes when subclass resources have been released.</returns>
    protected virtual ValueTask CleanupAsync() => ValueTask.CompletedTask;

    /// <summary>
    /// Cancels the accept loop, stops the underlying listener, awaits loop termination,
    /// runs <see cref="CleanupAsync"/>, and disposes the cancellation token source.
    /// </summary>
    /// <returns>A value task that completes when all resources have been fully released.</returns>
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
            catch (SocketException)
            { /* expected on listener.Stop; see the accept-loop filter for the platform split */
            }
            catch (IOException)
            { /* client disconnected during dispose */
            }
        }
        await CleanupAsync();
        Cts.Dispose();
    }
}
