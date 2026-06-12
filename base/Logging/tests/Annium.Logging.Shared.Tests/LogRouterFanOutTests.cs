using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Core.Runtime;
using Annium.Testing;
using Xunit;

namespace Annium.Logging.Shared.Tests;

/// <summary>
/// Verifies the fan-out behavior of <see cref="Annium.Logging.Shared.Internal.LogRouter{TContext}"/>:
/// <c>Send</c> routes each message to EVERY registered scheduler whose filter matches, and independently
/// rejects the message for schedulers whose filter does not match.
/// </summary>
public class LogRouterFanOutTests : TestBase
{
    public LogRouterFanOutTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// When two routes have disjoint level filters (A: Error-and-above, B: Info-and-below),
    /// an Info message is delivered only to sink B and an Error message is delivered only to sink A.
    /// </summary>
    [Fact]
    public void DisjointLevelFilters_InfoMessage_OnlyRouteB_Receives()
    {
        // arrange
        var sinkA = new CapturingSink();
        var sinkB = new CapturingSink();
        var subject = BuildTwoRouteProvider(sinkA, sinkB);

        // act
        subject.Info("hello");

        // assert — sinkA (Error+) must see nothing, sinkB (Info-) must see the message
        sinkA.Messages.IsEmpty();
        sinkB.Messages.Has(1);
        sinkB.Messages.At(0).Level.Is(LogLevel.Info);
        sinkB.Messages.At(0).Message.Is("hello");
    }

    /// <summary>
    /// When two routes have disjoint level filters (A: Error-and-above, B: Info-and-below),
    /// an Error message is delivered only to sink A and not to sink B.
    /// </summary>
    [Fact]
    public void DisjointLevelFilters_ErrorMessage_OnlyRouteA_Receives()
    {
        // arrange
        var sinkA = new CapturingSink();
        var sinkB = new CapturingSink();
        var subject = BuildTwoRouteProvider(sinkA, sinkB);

        // act
        subject.Error("boom");

        // assert — sinkA (Error+) must see the message, sinkB (Info-) must see nothing
        sinkA.Messages.Has(1);
        sinkA.Messages.At(0).Level.Is(LogLevel.Error);
        sinkA.Messages.At(0).Message.Is("boom");
        sinkB.Messages.IsEmpty();
    }

    /// <summary>
    /// When two routes both use ForAll() filters (fully overlapping), a single message is fan-out
    /// delivered to BOTH sinks regardless of which route is registered first.
    /// </summary>
    [Fact]
    public void OverlappingFilters_BothSinks_ReceiveMessage()
    {
        // arrange
        var sinkA = new CapturingSink();
        var sinkB = new CapturingSink();
        var subject = BuildOverlappingRouteProvider(sinkA, sinkB);

        // act
        subject.Info("broadcast");

        // assert — both sinks receive exactly one copy of the message
        sinkA.Messages.Has(1);
        sinkA.Messages.At(0).Message.Is("broadcast");
        sinkB.Messages.Has(1);
        sinkB.Messages.At(0).Message.Is("broadcast");
    }

    /// <summary>
    /// Fan-out delivers each message independently: when multiple messages are logged,
    /// both overlapping sinks accumulate all of them in order.
    /// </summary>
    [Fact]
    public void OverlappingFilters_MultipleMessages_AllDeliveredToBothSinks()
    {
        // arrange
        var sinkA = new CapturingSink();
        var sinkB = new CapturingSink();
        var subject = BuildOverlappingRouteProvider(sinkA, sinkB);

        // act
        subject.Info("first");
        subject.Warn("second");
        subject.Error("third");

        // assert — both sinks see all three messages
        sinkA.Messages.Has(3);
        sinkB.Messages.Has(3);

        sinkA.Messages.At(0).Message.Is("first");
        sinkA.Messages.At(1).Message.Is("second");
        sinkA.Messages.At(2).Message.Is("third");

        sinkB.Messages.At(0).Message.Is("first");
        sinkB.Messages.At(1).Message.Is("second");
        sinkB.Messages.At(2).Message.Is("third");
    }

    /// <summary>
    /// Builds a service provider with two disjoint level-filtered routes:
    /// route A captures only Error-and-above into <paramref name="sinkA"/>;
    /// route B captures only Info-and-below (i.e. Trace/Debug/Info) into <paramref name="sinkB"/>.
    /// </summary>
    /// <param name="sinkA">The capturing handler for route A (Error-and-above).</param>
    /// <param name="sinkB">The capturing handler for route B (Info-and-below).</param>
    /// <returns>An <see cref="ILogSubject"/> backed by the built service provider with two disjoint routes.</returns>
    private static ILogSubject BuildTwoRouteProvider(CapturingSink sinkA, CapturingSink sinkB)
    {
        var container = new ServiceContainer();
        container.AddTime().WithManagedTime().SetDefault();
        container.AddLogging<Context>();
        var provider = container.BuildServiceProvider();

        provider.UseLogging<Context>(route =>
        {
            route.For(m => m.Level >= LogLevel.Error).Use(sinkA);
            route.For(m => m.Level <= LogLevel.Info).Use(sinkB);
        });

        return provider.Resolve<ILogBridgeFactory>().Get("test");
    }

    /// <summary>
    /// Builds a service provider with two ForAll() routes — fully overlapping — both pointing
    /// to independent sinks so that fan-out can be observed.
    /// </summary>
    /// <param name="sinkA">The capturing handler for the first ForAll() route.</param>
    /// <param name="sinkB">The capturing handler for the second ForAll() route.</param>
    /// <returns>An <see cref="ILogSubject"/> backed by the built service provider with two overlapping routes.</returns>
    private static ILogSubject BuildOverlappingRouteProvider(CapturingSink sinkA, CapturingSink sinkB)
    {
        var container = new ServiceContainer();
        container.AddTime().WithManagedTime().SetDefault();
        container.AddLogging<Context>();
        var provider = container.BuildServiceProvider();

        provider.UseLogging<Context>(route =>
        {
            route.ForAll().Use(sinkA);
            route.ForAll().Use(sinkB);
        });

        return provider.Resolve<ILogBridgeFactory>().Get("test");
    }

    /// <summary>
    /// Capturing spy that accumulates every received <see cref="LogMessage{TContext}"/>.
    /// Thread-safe via lock; uses an immediate (non-buffering) handler so dispatch is
    /// synchronous and no async wait is needed in assertions.
    /// </summary>
    private sealed class CapturingSink : ILogHandler<Context>
    {
        /// <summary>
        /// Accumulated messages in delivery order.
        /// </summary>
        public List<LogMessage<Context>> Messages { get; } = new();

        /// <summary>
        /// Records the incoming batch of log messages into <see cref="Messages"/>.
        /// </summary>
        /// <param name="messages">The log message batch to capture.</param>
        /// <param name="ct">The cancellation token (unused).</param>
        /// <returns>A completed <see cref="ValueTask"/>.</returns>
        public ValueTask HandleAsync(IReadOnlyList<LogMessage<Context>> messages, CancellationToken ct)
        {
            lock (Messages)
                foreach (var msg in messages)
                    Messages.Add(msg);

            return ValueTask.CompletedTask;
        }
    }

    /// <summary>
    /// Dedicated log context for fan-out tests; keeps them isolated from tests that use
    /// <see cref="DefaultLogContext"/>.
    /// </summary>
    private class Context;
}
