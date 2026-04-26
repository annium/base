using System;
using System.Collections.Generic;
using System.Linq;
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

        protected override ValueTask<bool> SendEventsAsync(IReadOnlyCollection<LogMessage<DefaultLogContext>> events) =>
            new(true);
    }
}
