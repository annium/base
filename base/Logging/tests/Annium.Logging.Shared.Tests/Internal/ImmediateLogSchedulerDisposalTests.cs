using System.Threading.Tasks;
using Annium.Logging.Shared.Internal;
using Annium.Testing;
using Xunit;

namespace Annium.Logging.Shared.Tests.Internal;

/// <summary>
/// Tests for <see cref="ImmediateLogScheduler{TContext}"/> disposal behavior:
/// IDisposable spy is disposed exactly once, IAsyncDisposable spy is disposed exactly once,
/// and a second DisposeAsync does not re-dispose and does not throw.
/// </summary>
public class ImmediateLogSchedulerDisposalTests
{
    /// <summary>
    /// DisposeAsync with a synchronous-disposable handler calls Dispose on that handler exactly once.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task DisposeAsync_WithIDisposableHandler_DisposesHandlerExactlyOnce()
    {
        var handler = new DisposableSink();
        var scheduler = new ImmediateLogScheduler<DefaultLogContext>(_ => true, handler);

        await scheduler.DisposeAsync();

        handler.DisposeCount.Is(1);
    }

    /// <summary>
    /// A second DisposeAsync with a synchronous-disposable handler does NOT call Dispose again.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task DisposeAsync_CalledTwice_WithIDisposableHandler_DisposesHandlerExactlyOnce()
    {
        var handler = new DisposableSink();
        var scheduler = new ImmediateLogScheduler<DefaultLogContext>(_ => true, handler);

        await scheduler.DisposeAsync();
        await scheduler.DisposeAsync();

        handler.DisposeCount.Is(1);
    }

    /// <summary>
    /// DisposeAsync with an async-disposable handler calls DisposeAsync on that handler exactly once.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task DisposeAsync_WithIAsyncDisposableHandler_DisposesHandlerExactlyOnce()
    {
        var handler = new AsyncDisposableSink();
        var scheduler = new ImmediateLogScheduler<DefaultLogContext>(_ => true, handler);

        await scheduler.DisposeAsync();

        handler.DisposeCount.Is(1);
    }

    /// <summary>
    /// A second DisposeAsync with an async-disposable handler does NOT call DisposeAsync again.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task DisposeAsync_CalledTwice_WithIAsyncDisposableHandler_DisposesHandlerExactlyOnce()
    {
        var handler = new AsyncDisposableSink();
        var scheduler = new ImmediateLogScheduler<DefaultLogContext>(_ => true, handler);

        await scheduler.DisposeAsync();
        await scheduler.DisposeAsync();

        handler.DisposeCount.Is(1);
    }
}
