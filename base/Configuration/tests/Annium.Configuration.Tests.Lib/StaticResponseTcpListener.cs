using System;
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
public sealed class StaticResponseTcpListener : TcpListenerBase
{
    private readonly HttpStatusCode _status;
    private readonly string _body;
    private readonly string _contentType;

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
        : base(resourcePath)
    {
        _status = status;
        _body = body;
        _contentType = contentType;
    }

    /// <inheritdoc />
    protected override async ValueTask HandleClientAsync(TcpClient client, CancellationToken ct)
    {
        using (client)
        {
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
            await stream.WriteAsync(header, ct);
            if (bodyBytes.Length > 0)
                await stream.WriteAsync(bodyBytes, ct);
            await stream.FlushAsync(ct);
        }
    }
}
