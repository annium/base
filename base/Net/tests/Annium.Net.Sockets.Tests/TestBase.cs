using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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
    /// Base port number for test servers
    /// </summary>
    private static int _basePort = 30000;

    /// <summary>
    /// The server URI for testing
    /// </summary>
    protected readonly Uri ServerUri;

    /// <summary>
    /// The endpoint for the test server
    /// </summary>
    protected readonly IPEndPoint EndPoint;

    /// <summary>
    /// The port number for this test instance
    /// </summary>
    private readonly int _port;

    /// <summary>
    /// Random number generator for test data
    /// </summary>
    private readonly Random _random = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="TestBase"/> class
    /// </summary>
    /// <param name="outputHelper">The test output helper</param>
    protected TestBase(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        _port = Interlocked.Increment(ref _basePort);
        ServerUri = new Uri($"tcp://127.0.0.1:{_port}");
        EndPoint = new IPEndPoint(IPAddress.Loopback, _port);
    }

    /// <summary>
    /// Runs a base server with the specified handler
    /// </summary>
    /// <param name="handle">The handler function for processing connections</param>
    /// <returns>An async disposable that stops the server when disposed</returns>
    protected IAsyncDisposable RunServerBase(Func<IServiceProvider, Socket, CancellationToken, Task> handle)
    {
        this.Trace("start");

        var sp = Get<IServiceProvider>();
        var server = ServerBuilder.New(sp, _port).WithHandler(new Handler(sp, handle)).Build();
        var cts = new CancellationTokenSource();

        this.Trace("run server");
        var serverTask = server.RunAsync(cts.Token);

        this.Trace("done");

        return Disposable.Create(async () =>
        {
            this.Trace("start");

            // await before cancellation for a while
            await Task.Delay(5, CancellationToken.None);

            this.Trace("cancel server run");
            await cts.CancelAsync();

            this.Trace("await server task");
#pragma warning disable VSTHRD003
            await serverTask;
#pragma warning restore VSTHRD003

            this.Trace("done");
        });
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
