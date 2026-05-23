using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Configuration.Tests.Lib;

/// <summary>
/// Local TCP listener that returns a fixed HTTP status code + body for any incoming request.
/// Used to drive non-2xx response paths without bringing up a full HTTP server.
/// </summary>
public sealed class StaticResponseTcpListener : IAsyncDisposable
{
    private readonly TcpListener _listener;
    private readonly CancellationTokenSource _cts = new();
    private readonly HttpStatusCode _status;
    private readonly string _body;
    private readonly string _resourcePath;
    private readonly string _contentType;
    private readonly TaskCompletionSource _listening = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private Task? _acceptLoop;

    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="status">HTTP status code to return.</param>
    /// <param name="body">Response body.</param>
    /// <param name="resourcePath">The resource path appended to the listener's URI (e.g. "config.json").</param>
    /// <param name="contentType">Response Content-Type header value.</param>
    public StaticResponseTcpListener(
        HttpStatusCode status,
        string body,
        string resourcePath,
        string contentType = "application/json"
    )
    {
        _status = status;
        _body = body;
        _resourcePath = resourcePath;
        _contentType = contentType;
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
                    using var client = await _listener.AcceptTcpClientAsync(_cts.Token);
                    await using var stream = client.GetStream();

                    // Drain the request headers (best-effort) before replying.
                    var buffer = new byte[4096];
                    try
                    {
                        using var readCts = new CancellationTokenSource(TimeSpan.FromMilliseconds(200));
                        _ = await stream.ReadAsync(buffer, readCts.Token);
                    }
                    catch (OperationCanceledException)
                    { /* best-effort header drain timed out; proceed to write response */
                    }

                    var bodyBytes = Encoding.UTF8.GetBytes(_body);
                    var reasonPhrase = _status.ToString();
                    var header = Encoding.ASCII.GetBytes(
                        $"HTTP/1.1 {(int)_status} {reasonPhrase}\r\n"
                            + $"Content-Length: {bodyBytes.Length}\r\n"
                            + $"Content-Type: {_contentType}\r\n"
                            + "Connection: close\r\n"
                            + "\r\n"
                    );
                    await stream.WriteAsync(header, _cts.Token);
                    if (bodyBytes.Length > 0)
                        await stream.WriteAsync(bodyBytes, _cts.Token);
                    await stream.FlushAsync(_cts.Token);
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
            catch (IOException)
            { /* client disconnected during dispose */
            }
        }
        _cts.Dispose();
    }
}
