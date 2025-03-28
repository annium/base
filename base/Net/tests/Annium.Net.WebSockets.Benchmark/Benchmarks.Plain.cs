using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Net.WebSockets.Benchmark.Internal;
using Annium.Net.WebSockets.Internal;
using BenchmarkDotNet.Attributes;
using NativeClientWebSocket = System.Net.WebSockets.ClientWebSocket;

namespace Annium.Net.WebSockets.Benchmark;

[MemoryDiagnoser]
public partial class Benchmarks
{
    private CancellationTokenSource _plainCts = default!;
    private ManualResetEventSlim _plainGate = default!;
    private long _plainEventCount;
    private NativeClientWebSocket _plainSocket = default!;
    private Task<WebSocketCloseResult> _plainListenTask = default!;

    [IterationSetup(Target = nameof(Plain))]
    public void IterationSetup_Plain()
    {
        _plainCts = new();
        _plainGate = new ManualResetEventSlim();
        _plainEventCount = Constants.TotalMessages;

        _plainSocket = new NativeClientWebSocket();
        var client = new ManagedWebSocket(_plainSocket, VoidLogger.Instance);
        client.OnTextReceived += HandleMessage_Plain;
        _plainSocket
            .ConnectAsync(new Uri($"ws://127.0.0.1:{Constants.Port}/"), CancellationToken.None)
            .GetAwaiter()
#pragma warning disable VSTHRD002
            .GetResult();
#pragma warning restore VSTHRD002
        _plainListenTask = client.ListenAsync(_plainCts.Token);
    }

    [IterationCleanup(Target = nameof(Plain))]
    public void IterationCleanup_Plain()
    {
#pragma warning disable VSTHRD110
        _plainSocket.CloseOutputAsync(
            System.Net.WebSockets.WebSocketCloseStatus.NormalClosure,
            string.Empty,
            CancellationToken.None
        );
#pragma warning restore VSTHRD110
        _plainCts.Cancel();
#pragma warning disable VSTHRD002
        _plainListenTask.Wait();
#pragma warning restore VSTHRD002
    }

    [Benchmark(Baseline = true)]
    public void Plain()
    {
        _plainGate.Wait();
    }

    private void HandleMessage_Plain(ReadOnlyMemory<byte> data)
    {
        if (Interlocked.Decrement(ref _plainEventCount) > 0)
        {
            return;
        }

        _plainGate.Set();
    }
}
