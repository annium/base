using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Net.Servers.Sockets;
using Xunit;

namespace Annium.Net.Sockets.Tests;

/// <summary>
/// Base class for socket testing with common functionality
/// </summary>
public abstract class TestBase : Testing.TestBase
{
    /// <summary>
    /// Random number generator for test data
    /// </summary>
    // seeded so generated message sizes/contents are deterministic and reproducible across runs
    // (a failure on a particular size can be re-run; a fresh TestBase per test gives each its own sequence).
    private readonly Random _random = new(12345);

    /// <summary>
    /// Initializes a new instance of the <see cref="TestBase"/> class
    /// </summary>
    /// <param name="outputHelper">The test output helper</param>
    protected TestBase(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Runs a base server with the specified handler
    /// </summary>
    /// <param name="handle">The handler function for processing connections</param>
    /// <returns>An async disposable that stops the server when disposed</returns>
    protected IServer RunServerBase(Func<IServiceProvider, Socket, CancellationToken, Task> handle)
    {
        this.Trace("start");

        var sp = Get<IServiceProvider>();
        var handler = new Handler(sp, handle);

        return ServerBuilder.New(sp).WithHandler(handler).Start().NotNull();
    }

    /// <summary>
    /// Wraps the raw server-side socket in an authenticated <see cref="SslStream"/> using the
    /// shared test cert (<c>keys/ecdsa_cert.pfx</c>). Caller takes ownership of the returned
    /// stream and must dispose it (typically via <c>await using</c>).
    /// </summary>
    /// <param name="raw">The raw server-side socket accepted by <see cref="RunServerBase"/>.</param>
    /// <param name="ct">Cancellation token (reserved for future use; current ssl authenticate overload does not accept one).</param>
    /// <returns>An authenticated <see cref="SslStream"/> wrapping <paramref name="raw"/>.</returns>
    protected async Task<SslStream> WrapAsServerSslStreamAsync(Socket raw, CancellationToken ct)
    {
        _ = ct;
        var cert = X509CertificateLoader.LoadPkcs12FromFile("keys/ecdsa_cert.pfx", []);
        var sslStream = new SslStream(new NetworkStream(raw), false);
        await sslStream.AuthenticateAsServerAsync(
            cert,
            clientCertificateRequired: false,
            checkCertificateRevocation: true
        );
        return sslStream;
    }

    /// <summary>
    /// Creates a plain (non-SSL) client-side stream over the connected socket. The returned
    /// <see cref="NetworkStream"/> owns the socket (<c>ownsSocket: true</c>) so disposing the stream
    /// in test teardown closes the socket.
    /// </summary>
    /// <param name="socket">The connected client socket.</param>
    /// <returns>A plain client stream owning <paramref name="socket"/>.</returns>
    protected static Task<Stream> CreatePlainClientStreamAsync(Socket socket)
    {
        return Task.FromResult<Stream>(new NetworkStream(socket, ownsSocket: true));
    }

    /// <summary>
    /// Creates an authenticated SSL client-side stream over the connected socket. The inner
    /// <see cref="NetworkStream"/> owns the socket (<c>ownsSocket: true</c>) and the
    /// <see cref="SslStream"/> owns the inner stream, so disposing the returned stream in test
    /// teardown closes the socket. Server certificate validation is disabled (tests use a self-signed cert).
    /// </summary>
    /// <param name="socket">The connected client socket.</param>
    /// <returns>An authenticated SSL client stream owning <paramref name="socket"/>.</returns>
    protected static async Task<Stream> CreateSslClientStreamAsync(Socket socket)
    {
        var networkStream = new NetworkStream(socket, ownsSocket: true);
        var sslStream = new SslStream(networkStream, false, ValidateServerCertificate, null);

        await sslStream.AuthenticateAsClientAsync(string.Empty);

        return sslStream;

        static bool ValidateServerCertificate(
            object sender,
            X509Certificate? certificate,
            X509Chain? chain,
            SslPolicyErrors sslPolicyErrors
        )
        {
            // by design, no ssl verification in tests (cause it will require valid SSL certificate)
            return true;
        }
    }

    /// <summary>
    /// Generates a message with specified size and chunks
    /// </summary>
    /// <param name="size">The total size of the message</param>
    /// <param name="chunkAverageSize">The average size of each chunk</param>
    /// <returns>A tuple containing the complete message and its chunks</returns>
    protected (byte[] message, IReadOnlyList<byte[]> chunks) GenerateMessage(int size, int chunkAverageSize)
    {
        var minChunkSize = (int)Math.Floor((double)chunkAverageSize / 2);
        var maxChunkSize = minChunkSize * 3;
        var chunks = new List<byte[]>();
        var chunksTotalSize = 0;

        while (chunksTotalSize < size)
        {
            var chunkSize = Math.Min(_random.Next(minChunkSize, maxChunkSize), size - chunksTotalSize);
            chunksTotalSize += chunkSize;

            var chunk = new byte[chunkSize];
            _random.NextBytes(chunk);
            chunks.Add(chunk);
        }

        var message = chunks.SelectMany(x => x).ToArray();

        return (message, chunks);
    }

    /// <summary>
    /// Generates multiple messages with random sizes
    /// </summary>
    /// <param name="count">The number of messages to generate</param>
    /// <param name="averageSize">The average size of each message</param>
    /// <returns>A collection of generated messages</returns>
    protected IReadOnlyList<byte[]> GenerateMessages(int count, int averageSize)
    {
        var minSize = (int)Math.Floor((double)averageSize / 2);
        var maxSize = minSize * 3;
        var messages = new List<byte[]>(count);

        for (var i = 0; i < count; i++)
        {
            var message = new byte[_random.Next(minSize, maxSize)];
            _random.NextBytes(message);
            messages.Add(message);
        }

        return messages;
    }
}

/// <summary>
/// Handler implementation for socket connections
/// </summary>
file class Handler : IHandler
{
    /// <summary>
    /// The service provider
    /// </summary>
    private readonly IServiceProvider _sp;

    /// <summary>
    /// The handler function for processing sockets
    /// </summary>
    private readonly Func<IServiceProvider, Socket, CancellationToken, Task> _handle;

    /// <summary>
    /// Initializes a new instance of the <see cref="Handler"/> class
    /// </summary>
    /// <param name="sp">The service provider</param>
    /// <param name="handle">The handler function for processing sockets</param>
    public Handler(IServiceProvider sp, Func<IServiceProvider, Socket, CancellationToken, Task> handle)
    {
        _sp = sp;
        _handle = handle;
    }

    /// <summary>
    /// Handles a socket connection asynchronously
    /// </summary>
    /// <param name="socket">The socket to handle</param>
    /// <param name="ct">The cancellation token</param>
    /// <returns>A task representing the handling operation</returns>
    public Task HandleAsync(Socket socket, CancellationToken ct)
    {
        return _handle(_sp, socket, ct);
    }
}
