using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Core.Runtime;
using Annium.Logging.Shared.Internal;
using Annium.Testing;
using Xunit;

namespace Annium.Logging.Shared.Tests.Internal;

/// <summary>
/// Verifies <see cref="LogRoute{TContext}.Use(ILogHandler{TContext}, LogRouteConfiguration?)"/> auto-picks
/// <see cref="ImmediateLogScheduler{TContext}"/> for non-buffering handlers and
/// <see cref="BackgroundLogScheduler{TContext}"/> for handlers derived from
/// <see cref="BufferingLogHandler{TContext}"/>; and that the fluent override hooks force the alternative.
/// </summary>
public class LogRouteSchedulerSelectionTests
{
    /// <summary>
    /// A non-buffering handler should route through ImmediateLogScheduler by default.
    /// </summary>
    [Fact]
    public void Use_NonBufferingHandler_DispatchesViaImmediate()
    {
        var schedulers = BuildSchedulers(route => route.Use(new SyncSink()));

        schedulers.Has(1);
        schedulers.At(0).As<ImmediateLogScheduler<DefaultLogContext>>();
    }

    /// <summary>
    /// A handler derived from BufferingLogHandler should route through BackgroundLogScheduler by default.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task Use_BufferingHandler_DispatchesViaBackground()
    {
        var schedulers = BuildSchedulers(route => route.Use(new BufferingSink()));

        schedulers.Has(1);
        schedulers.At(0).As<BackgroundLogScheduler<DefaultLogContext>>();

        // BackgroundLogScheduler is IAsyncDisposable; dispose it so the test doesn't leak the
        // pump task across runs.
        await ((IAsyncDisposable)schedulers.At(0)).DisposeAsync();
    }

    /// <summary>
    /// A non-buffering handler with .WithBackgroundScheduler() must route through BackgroundLogScheduler.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task Use_NonBufferingHandler_WithBackgroundScheduler_OverridesToBackground()
    {
        var schedulers = BuildSchedulers(route => route.Use(new SyncSink()).WithBackgroundScheduler());

        schedulers.Has(1);
        schedulers.At(0).As<BackgroundLogScheduler<DefaultLogContext>>();

        await ((IAsyncDisposable)schedulers.At(0)).DisposeAsync();
    }

    /// <summary>
    /// Builds a service provider, applies the route configuration, and returns the schedulers list.
    /// </summary>
    /// <param name="configure">An action that configures the log route under test.</param>
    /// <returns>The resolved list of <see cref="ILogScheduler{TContext}"/> instances registered by the route.</returns>
    private static IReadOnlyList<ILogScheduler<DefaultLogContext>> BuildSchedulers(
        Action<LogRoute<DefaultLogContext>> configure
    )
    {
        var container = new ServiceContainer();
        container.AddTime().WithManagedTime().SetDefault();
        container.AddLogging<DefaultLogContext>();

        var provider = container.BuildServiceProvider();

        provider.UseLogging<DefaultLogContext>(configure);

        return provider.Resolve<List<ILogScheduler<DefaultLogContext>>>();
    }

    /// <summary>
    /// Minimal non-buffering sink for selection tests.
    /// </summary>
    private sealed class SyncSink : ILogHandler<DefaultLogContext>
    {
        /// <summary>
        /// Completes immediately, discarding all messages — used only to verify scheduler selection.
        /// </summary>
        /// <param name="messages">The log messages passed by the scheduler.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A completed <see cref="ValueTask"/>.</returns>
        public ValueTask HandleAsync(IReadOnlyList<LogMessage<DefaultLogContext>> messages, CancellationToken ct) =>
            ValueTask.CompletedTask;
    }

    /// <summary>
    /// Minimal buffering sink — never sends, always buffers, just exists to verify scheduler selection.
    /// </summary>
    private sealed class BufferingSink : BufferingLogHandler<DefaultLogContext>
    {
        public BufferingSink()
            : base(new LogRouteConfiguration()) { }

        /// <summary>
        /// Signals that all buffered events were handled — this sink never actually sends anything,
        /// it exists solely to exercise the buffering-handler scheduler-selection path.
        /// </summary>
        /// <param name="events">The buffered log messages to send.</param>
        /// <returns>A <see cref="ValueTask{TResult}"/> that always resolves to <c>true</c>.</returns>
        protected override ValueTask<bool> SendEventsAsync(IReadOnlyCollection<LogMessage<DefaultLogContext>> events) =>
            new(true);
    }
}
