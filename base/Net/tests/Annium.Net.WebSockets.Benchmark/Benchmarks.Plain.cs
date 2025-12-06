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
/// WebSocket benchmarks using plain event handlers for receiving messages
/// </summary>
[MemoryDiagnoser]
public partial class Benchmarks
{
    /// <summary>
    /// Cancellation token source for plain benchmark
    /// </summary>
    private CancellationTokenSource _plainCts = null!;

    /// <summary>
    /// Manual reset event for synchronizing plain benchmark completion
    /// </summary>
    private ManualResetEventSlim _plainGate = null!;

    /// <summary>
    /// Counter for plain events received
    /// </summary>
    private long _plainEventCount;

    /// <summary>
    /// Native client WebSocket for plain benchmark
    /// </summary>
    private NativeClientWebSocket _plainSocket = null!;

    /// <summary>
    /// Task for listening to plain WebSocket messages
    /// </summary>
    private Task<WebSocketCloseResult> _plainListenTask = null!;

    /// <summary>
    /// Sets up the plain benchmark iteration
    /// </summary>
    [IterationSetup(Target = nameof(Plain))]
    public void IterationSetup_Plain()
    {
        _plainCts = new();
        _plainGate = new ManualResetEventSlim();
        _plainEventCount = Constants.TotalMessages;

        _plainSocket = new NativeClientWebSocket();
        var client = new ManagedWebSocket(_plainSocket, VoidLogger.Instance);
        client.OnTextReceived += HandleMessage_Plain;
        _plainSocket.ConnectAsync(WorkloadServer.Uri, CancellationToken.None).GetAwaiter()
#pragma warning disable VSTHRD002
        .GetResult();
#pragma warning restore VSTHRD002
        _plainListenTask = client.ListenAsync(_plainCts.Token);
    }

    /// <summary>
    /// Cleans up the plain benchmark iteration
    /// </summary>
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

    /// <summary>
    /// Benchmark for plain event handler WebSocket message handling
    /// </summary>
    [Benchmark(Baseline = true)]
    public void Plain()
    {
        _plainGate.Wait();
    }

    /// <summary>
    /// Handles plain WebSocket message events
    /// </summary>
    /// <param name="data">The message data received</param>
    private void HandleMessage_Plain(ReadOnlyMemory<byte> data)
    {
        if (Interlocked.Decrement(ref _plainEventCount) > 0)
        {
            return;
        }

        _plainGate.Set();
    }
}
