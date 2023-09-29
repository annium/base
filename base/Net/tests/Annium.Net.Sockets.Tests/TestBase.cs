using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Net.Servers;
using Xunit.Abstractions;

namespace Annium.Net.Sockets.Tests;

public abstract class TestBase : Testing.Lib.TestBase
{
    private static int _basePort = 10000;
    protected readonly IPEndPoint EndPoint;
    private readonly int _port;
    private readonly Random _random = new();

    protected TestBase(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _port = Interlocked.Increment(ref _basePort);
        EndPoint = new IPEndPoint(IPAddress.Loopback, _port);
    }

    protected IAsyncDisposable RunServerBase(Func<IServiceProvider, Socket, CancellationToken, Task> handle)
    {
        this.Trace("start");

        var server = SocketServerBuilder.New(Get<IServiceProvider>(), _port).WithHandler(handle).Build();
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
            cts.Cancel();

            this.Trace("await server task");
            await serverTask;

            this.Trace("cancel");
        });
    }

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
}