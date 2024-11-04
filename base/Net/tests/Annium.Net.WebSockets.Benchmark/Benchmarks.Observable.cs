using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Net.WebSockets.Benchmark.Internal;
using Annium.Net.WebSockets.Internal;
using BenchmarkDotNet.Attributes;
using NativeClientWebSocket = System.Net.WebSockets.ClientWebSocket;

namespace Annium.Net.WebSockets.Benchmark;

public partial class Benchmarks
{
    private CancellationTokenSource _observableCts = default!;
    private ManualResetEventSlim _observableGate = default!;
    private long _observableEventCount;
    private NativeClientWebSocket _observableSocket = default!;
    private Task<WebSocketCloseResult> _observableListenTask = default!;

    [IterationSetup(Target = nameof(Observable))]
    public void IterationSetup_Observable()
    {
        _observableCts = new();
        _observableGate = new ManualResetEventSlim();
        _observableEventCount = Constants.TotalMessages;

        _observableSocket = new NativeClientWebSocket();
        var client = new ManagedWebSocket(_observableSocket, VoidLogger.Instance);
        client.ObserveText().Subscribe(HandleMessage_Observable);
        _observableSocket
            .ConnectAsync(new Uri($"ws://127.0.0.1:{Constants.Port}/"), CancellationToken.None)
            .GetAwaiter()
#pragma warning disable VSTHRD002
            .GetResult();
#pragma warning restore VSTHRD002
        _observableListenTask = client.ListenAsync(_observableCts.Token);
    }

    [IterationCleanup(Target = nameof(Observable))]
    public void IterationCleanup_Observable()
    {
#pragma warning disable VSTHRD110
        _observableSocket.CloseOutputAsync(
            System.Net.WebSockets.WebSocketCloseStatus.NormalClosure,
            string.Empty,
            CancellationToken.None
        );
#pragma warning restore VSTHRD110
        _observableCts.Cancel();
#pragma warning disable VSTHRD002
        _observableListenTask.Wait();
#pragma warning restore VSTHRD002
    }

    [Benchmark]
    public void Observable()
    {
        _observableGate.Wait();
    }

    private void HandleMessage_Observable(ReadOnlyMemory<byte> data)
    {
        if (Interlocked.Decrement(ref _observableEventCount) > 0)
        {
            return;
        }

        _observableGate.Set();
    }
}
