using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Core.Runtime;
using Annium.Testing;
using Xunit;

namespace Annium.Logging.Shared.Tests.Internal;

/// <summary>
/// Verifies the <see cref="LogRoute{TContext}"/> defer-to-Use registration semantics:
/// the constructor is side-effect-free; only <see cref="LogRoute{TContext}.Use(ILogHandler{TContext}, LogRouteConfiguration?)"/>
/// (or its factory overload) registers the route, and only on the first call. A second
/// call throws <see cref="InvalidOperationException"/>. The post-Use builder mutation
/// (<c>WithImmediateScheduler</c> / <c>WithBackgroundScheduler</c>) remains visible on the
/// already-registered instance because the routes list holds the reference.
/// </summary>
public class LogRouteRegistrationTests
{
    /// <summary>
    /// Calling <c>For(filter)</c> creates a new <see cref="LogRoute{TContext}"/> instance
    /// but does NOT register it; abandoning the result must leave the routes list empty.
    /// </summary>
    [Fact]
    public void For_WithoutUse_NotRegistered()
    {
        var (route, registered) = Build();

        _ = route.For(_ => true);

        registered.Has(0);
    }

    /// <summary>
    /// Calling <c>ForAll()</c> creates a new <see cref="LogRoute{TContext}"/> instance
    /// but does NOT register it; abandoning the result must leave the routes list empty.
    /// </summary>
    [Fact]
    public void ForAll_WithoutUse_NotRegistered()
    {
        var (route, registered) = Build();

        _ = route.ForAll();

        registered.Has(0);
    }

    /// <summary>
    /// A complete <c>For(filter).Use(handler)</c> chain registers exactly one route on the
    /// instance returned by <c>For</c> with the supplied filter, handler, and a default
    /// configuration. The original parent instance remains unregistered.
    /// </summary>
    [Fact]
    public void Use_AfterFor_RegistersOnceWithExpectedState()
    {
        var (route, registered) = Build();
        Func<LogMessage<DefaultLogContext>, bool> filter = _ => true;
        var handler = new SyncSink();

        route.For(filter).Use(handler);

        registered.Has(1);
        registered[0].Filter.Is(filter);
        registered[0].Handler.Is(handler);
        registered[0].Configuration.IsNotDefault();
    }

    /// <summary>
    /// The factory overload of <c>Use</c> registers identically to the direct overload:
    /// a single route with the factory-resolved handler.
    /// </summary>
    [Fact]
    public void Use_FactoryOverload_RegistersOnce()
    {
        var (route, registered) = Build();
        var handler = new SyncSink();

        route.For(_ => true).Use(_ => handler);

        registered.Has(1);
        registered[0].Handler.Is(handler);
    }

    /// <summary>
    /// Each <see cref="LogRoute{TContext}"/> may be configured at most once. A second
    /// call to <c>Use(...)</c> on the same instance must throw
    /// <see cref="InvalidOperationException"/>; the routes list state must be unchanged
    /// from after the first <c>Use</c> call.
    /// </summary>
    [Fact]
    public void Use_CalledTwice_Throws()
    {
        var (route, registered) = Build();
        var configured = route.For(_ => true);
        configured.Use(new SyncSink());

        Wrap.It(() => configured.Use(new SyncSink())).Throws<InvalidOperationException>();

        registered.Has(1);
    }

    /// <summary>
    /// The factory overload of <c>Use</c> must enforce the same single-configuration
    /// invariant: a second call on the same instance throws
    /// <see cref="InvalidOperationException"/> regardless of which overload was used first
    /// (factory then factory).
    /// </summary>
    [Fact]
    public void Use_FactoryOverload_CalledTwice_Throws()
    {
        var (route, registered) = Build();
        var configured = route.For(_ => true);
        configured.Use(_ => new SyncSink());

        Wrap.It(() => configured.Use(_ => new SyncSink())).Throws<InvalidOperationException>();

        registered.Has(1);
    }

    /// <summary>
    /// The post-<c>Use</c> builder hook <c>WithImmediateScheduler</c> mutates the
    /// already-registered route reference; the override must be observable on the
    /// instance held by the routes list.
    /// </summary>
    [Fact]
    public void WithImmediateScheduler_AfterUse_OverrideVisible()
    {
        var (route, registered) = Build();

        route.For(_ => true).Use(new SyncSink()).WithImmediateScheduler();

        registered.Has(1);
        registered[0].SchedulerOverride.Is(LogRouteSchedulerKind.Immediate);
    }

    /// <summary>
    /// Builds an empty registrations list and a parent <see cref="LogRoute{TContext}"/>
    /// whose register-action appends to it. The parent itself is never registered (the
    /// new defer-to-Use semantics) — the list captures only routes that get a terminal
    /// <c>Use(...)</c> call.
    /// </summary>
    /// <returns>The parent route + the list it registers into.</returns>
    private static (LogRoute<DefaultLogContext> Route, List<LogRoute<DefaultLogContext>> Registered) Build()
    {
        var container = new ServiceContainer();
        container.AddTime().WithManagedTime().SetDefault();
        var sp = container.BuildServiceProvider();

        var registered = new List<LogRoute<DefaultLogContext>>();
        var route = new LogRoute<DefaultLogContext>(sp, registered.Add);
        return (route, registered);
    }

    /// <summary>
    /// Minimal non-buffering log handler — used as a placeholder handler for registration
    /// assertions. The body is never invoked because tests assert on the routes list,
    /// not on dispatch behaviour.
    /// </summary>
    private sealed class SyncSink : ILogHandler<DefaultLogContext>
    {
        public ValueTask HandleAsync(IReadOnlyList<LogMessage<DefaultLogContext>> messages, CancellationToken ct) =>
            ValueTask.CompletedTask;
    }
}
