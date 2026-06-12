using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Core.Runtime;
using Annium.Logging.Shared.Tests.Internal;
using Annium.Testing;
using Xunit;

namespace Annium.Logging.Shared.Tests;

/// <summary>
/// Integration tests verifying that disposing the built <see cref="IServiceProviderContainer"/>
/// fires <c>OnDisposed</c> which in turn disposes every registered scheduler — and, transitively,
/// the handler owned by each scheduler — exactly once.
/// </summary>
public class ProviderOnDisposedChainTests
{
    /// <summary>
    /// An immediate (non-buffering) spy handler wired via <c>ForAll().Use(spy)</c>:
    /// disposing the provider disposes the immediate scheduler which
    /// disposes the IDisposable spy handler exactly once.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task ProviderDispose_WithImmediateSchedulerAndIDisposableSpy_DisposesHandlerOnce()
    {
        var spy = new DisposableSink();

        var provider = BuildProvider();
        provider.UseLogging<DefaultLogContext>(route => route.ForAll().Use(spy));

        await provider.DisposeAsync();

        spy.DisposeCount.Is(1);
    }

    /// <summary>
    /// An immediate (non-buffering) spy handler wired via <c>ForAll().Use(spy)</c>:
    /// disposing the provider disposes the immediate scheduler which
    /// disposes the IAsyncDisposable spy handler exactly once.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task ProviderDispose_WithImmediateSchedulerAndIAsyncDisposableSpy_DisposesHandlerOnce()
    {
        var spy = new AsyncDisposableSink();

        var provider = BuildProvider();
        provider.UseLogging<DefaultLogContext>(route => route.ForAll().Use(spy));

        await provider.DisposeAsync();

        spy.DisposeCount.Is(1);
    }

    /// <summary>
    /// A buffering spy (background-scheduler path) wired via <c>ForAll().Use(spy)</c>:
    /// disposing the provider disposes the background scheduler which
    /// disposes the IAsyncDisposable spy handler exactly once.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task ProviderDispose_WithBackgroundSchedulerAndBufferingSpy_DisposesHandlerOnce()
    {
        var spy = new BufferingAsyncDisposableSpy();

        var provider = BuildProvider();
        provider.UseLogging<DefaultLogContext>(route => route.ForAll().Use(spy));

        await provider.DisposeAsync();

        spy.DisposeCount.Is(1);
    }

    /// <summary>
    /// Builds a fresh <see cref="IServiceProviderContainer"/> with time and logging registered,
    /// ready to have routes configured via <c>UseLogging</c>.
    /// </summary>
    /// <returns>A new <see cref="IServiceProviderContainer"/> instance.</returns>
    private static IServiceProviderContainer BuildProvider()
    {
        var container = new ServiceContainer();
        container.AddTime().WithManagedTime().SetDefault();
        container.AddLogging<DefaultLogContext>();
        return container.BuildServiceProvider();
    }

    /// <summary>
    /// Buffering spy (derived from <see cref="BufferingLogHandler{TContext}"/>) implementing
    /// <see cref="IAsyncDisposable"/>. Routes through the background scheduler path and counts
    /// DisposeAsync calls.
    /// </summary>
    private sealed class BufferingAsyncDisposableSpy : BufferingLogHandler<DefaultLogContext>, IAsyncDisposable
    {
        /// <summary>Number of times DisposeAsync was called.</summary>
        private int _disposeCount;

        /// <summary>Thread-safe snapshot of DisposeAsync invocations.</summary>
        public int DisposeCount => Volatile.Read(ref _disposeCount);

        /// <summary>Initializes a new instance with a 10 ms / 1-item buffer window.</summary>
        public BufferingAsyncDisposableSpy()
            : base(new LogRouteConfiguration { BufferTime = TimeSpan.FromMilliseconds(10), BufferCount = 1 }) { }

        /// <summary>Increments the disposal counter and completes synchronously.</summary>
        /// <returns>A completed <see cref="ValueTask"/>.</returns>
        public ValueTask DisposeAsync()
        {
            Interlocked.Increment(ref _disposeCount);
            return ValueTask.CompletedTask;
        }

        /// <summary>Always reports success — events are not examined by these tests.</summary>
        /// <param name="events">The buffered events (ignored).</param>
        /// <returns>A <see cref="ValueTask{TResult}"/> that always resolves to <c>true</c>.</returns>
        protected override ValueTask<bool> SendEventsAsync(IReadOnlyCollection<LogMessage<DefaultLogContext>> events) =>
            new(true);
    }
}
