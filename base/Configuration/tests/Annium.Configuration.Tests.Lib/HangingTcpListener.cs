using System;
using System.Collections.Generic;
using System.Net;
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
public sealed class HangingTcpListener : IAsyncDisposable
{
    private readonly TcpListener _listener;
    private readonly CancellationTokenSource _cts = new();
    private readonly List<TcpClient> _accepted = new();
    private readonly TaskCompletionSource _listening = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly string _resourcePath;
    private Task? _acceptLoop;

    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="resourcePath">The resource path appended to the listener's URI (e.g. "config.json").</param>
    public HangingTcpListener(string resourcePath)
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
                while (!_cts.IsCancellationRequested)
                {
                    var client = await _listener.AcceptTcpClientAsync(_cts.Token);
                    // Hold a strong reference so GC doesn't reap the socket while the test
                    // is mid-request — otherwise HttpClient sees an IO error, not a timeout.
                    lock (_accepted)
                        _accepted.Add(client);
                }
            }
            catch (OperationCanceledException)
            { /* expected on dispose */
            }
            catch (ObjectDisposedException)
            { /* expected on dispose */
            }
        });

        await _listening.Task.WaitAsync(ct);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await _cts.CancelAsync();
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
        }
        lock (_accepted)
        {
            foreach (var c in _accepted)
                c.Dispose();
            _accepted.Clear();
        }
        _cts.Dispose();
    }
}
