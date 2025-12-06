using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Net.WebSockets.Benchmark.Internal;
using Annium.Net.WebSockets.Internal;
using BenchmarkDotNet.Attributes;
using NativeClientWebSocket = System.Net.WebSockets.ClientWebSocket;

namespace Annium.Net.WebSockets.Benchmark;

/// <summary>
/// WebSocket benchmarks using observable pattern for receiving messages
/// </summary>
public partial class Benchmarks
{
    /// <summary>
    /// Cancellation token source for observable benchmark
    /// </summary>
    private CancellationTokenSource _observableCts = null!;

    /// <summary>
    /// Manual reset event for synchronizing observable benchmark completion
    /// </summary>
    private ManualResetEventSlim _observableGate = null!;

    /// <summary>
    /// Counter for observable events received
    /// </summary>
    private long _observableEventCount;

    /// <summary>
    /// Native client WebSocket for observable benchmark
    /// </summary>
    private NativeClientWebSocket _observableSocket = null!;

    /// <summary>
    /// Task for listening to observable WebSocket messages
    /// </summary>
    private Task<WebSocketCloseResult> _observableListenTask = null!;

    /// <summary>
    /// Sets up the observable benchmark iteration
    /// </summary>
    [IterationSetup(Target = nameof(Observable))]
    public void IterationSetup_Observable()
    {
        _observableCts = new();
        _observableGate = new ManualResetEventSlim();
        _observableEventCount = Constants.TotalMessages;

        _observableSocket = new NativeClientWebSocket();
        var client = new ManagedWebSocket(_observableSocket, VoidLogger.Instance);
        client.ObserveText().Subscribe(HandleMessage_Observable);
        _observableSocket.ConnectAsync(WorkloadServer.Uri, CancellationToken.None).GetAwaiter()
#pragma warning disable VSTHRD002
        .GetResult();
#pragma warning restore VSTHRD002
        _observableListenTask = client.ListenAsync(_observableCts.Token);
    }

    /// <summary>
    /// Cleans up the observable benchmark iteration
    /// </summary>
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

    /// <summary>
    /// Benchmark for observable pattern WebSocket message handling
    /// </summary>
    [Benchmark]
    public void Observable()
    {
        _observableGate.Wait();
    }

    /// <summary>
    /// Handles observable WebSocket message events
    /// </summary>
    /// <param name="data">The message data received</param>
    private void HandleMessage_Observable(ReadOnlyMemory<byte> data)
    {
        if (Interlocked.Decrement(ref _observableEventCount) > 0)
        {
            return;
        }

        _observableGate.Set();
    }
}
