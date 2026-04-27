using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Core.Runtime;
using Annium.Logging.InMemory;
using Annium.Logging.Shared;
using Annium.Testing;
using Annium.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using AnniumLogLevel = Annium.Logging.LogLevel;
using IMicrosoftLogger = Microsoft.Extensions.Logging.ILogger;
using IMicrosoftLoggerProvider = Microsoft.Extensions.Logging.ILoggerProvider;
using MicrosoftLogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Annium.Logging.Microsoft.Tests;

/// <summary>
/// Tests verifying that the Microsoft.Extensions.Logging bridge respects the configured route
/// filter — both via <c>IsEnabled</c> short-circuit and via end-to-end log dispatch.
/// </summary>
public class LoggingBridgeTests
{
    /// <summary>
    /// With a Warn-or-above route filter, the bridge reports Information as disabled — letting
    /// MS callers short-circuit log construction.
    /// </summary>
    [Fact]
    public void IsEnabled_WhenRouteFilterRejectsLevel_ReturnsFalse()
    {
        var (logger, _) = BuildBridge(filterMinLevel: AnniumLogLevel.Warn);

        logger.IsEnabled(MicrosoftLogLevel.Information).IsFalse();
    }

    /// <summary>
    /// With a Trace-or-above route filter, the bridge reports Information as enabled.
    /// </summary>
    [Fact]
    public void IsEnabled_WhenRouteFilterAcceptsLevel_ReturnsTrue()
    {
        var (logger, _) = BuildBridge(filterMinLevel: AnniumLogLevel.Trace);

        logger.IsEnabled(MicrosoftLogLevel.Information).IsTrue();
    }

    /// <summary>
    /// End-to-end: a Warn-or-above route drops MS Information logs before they reach the in-memory sink.
    /// MS callers respect <c>IsEnabled</c>, so when the bridge reports false they don't construct
    /// the message at all — the InMemory sink stays empty.
    /// </summary>
    [Fact]
    public void Log_BelowFilterLevel_DoesNotReachInMemorySink()
    {
        var (logger, sink) = BuildBridge(filterMinLevel: AnniumLogLevel.Warn);

        logger.Log(MicrosoftLogLevel.Information, "should be dropped");

        sink.Logs.Count.Is(0);
    }

    /// <summary>
    /// Per Microsoft.Extensions.Logging convention, <see cref="MicrosoftLogLevel.None"/>
    /// means "no log". The bridge must report it as disabled directly, even with the
    /// most permissive (Trace-or-above) route filter — without consulting the sentry.
    /// </summary>
    [Fact]
    public void IsEnabled_None_ReturnsFalseWithoutSentry()
    {
        var (logger, _) = BuildBridge(filterMinLevel: AnniumLogLevel.Trace);

        logger.IsEnabled(MicrosoftLogLevel.None).IsFalse();
    }

    /// <summary>
    /// Per Microsoft.Extensions.Logging convention, <see cref="MicrosoftLogLevel.None"/>
    /// is a sentinel that should never produce a log entry. Even with the most permissive
    /// (Trace-or-above) route filter, calls at <c>None</c> level must be a no-op and never
    /// reach the in-memory sink.
    /// </summary>
    [Fact]
    public void Log_None_DoesNotReachSink()
    {
        var (logger, sink) = BuildBridge(filterMinLevel: AnniumLogLevel.Trace);

        logger.Log(MicrosoftLogLevel.None, "should be dropped");

        sink.Logs.Count.Is(0);
    }

    /// <summary>
    /// Positive dispatch: with a Trace-or-above route filter, an MS-Information log call
    /// must reach the in-memory sink. Confirms <see cref="LoggingBridge.Log{TState}"/>
    /// actually invokes <c>_sentryBridge.Register(...)</c> on the non-short-circuit path —
    /// guards against silent regressions where the early-return inadvertently captures a
    /// non-<c>None</c> level.
    /// </summary>
    [Fact]
    public async Task Log_AboveFilterLevel_ReachesInMemorySink()
    {
        var (logger, sink) = BuildBridge(filterMinLevel: AnniumLogLevel.Trace);

        logger.LogInformation("should be delivered");

        await Wait.UntilAsync(() => sink.Logs.Count == 1);
        sink.Logs.At(0).Message.Is("should be delivered");
    }

    /// <summary>
    /// Wires up an MS logger bridge against an InMemory sink filtered at the given level.
    /// </summary>
    /// <param name="filterMinLevel">Minimum level the route filter accepts.</param>
    /// <returns>The MS logger bridge and the InMemory sink it ultimately routes to.</returns>
    private static (IMicrosoftLogger Logger, InMemoryLogHandler<DefaultLogContext> Sink) BuildBridge(
        AnniumLogLevel filterMinLevel
    )
    {
        var container = new ServiceContainer();
        container.AddTime().WithManagedTime().SetDefault();
        container.AddLogging<DefaultLogContext>();
        container.Collection.AddLoggingBridge();

        var provider = container.BuildServiceProvider();

        var sink = new InMemoryLogHandler<DefaultLogContext>();
        provider.UseLogging<DefaultLogContext>(route => route.For(m => m.Level >= filterMinLevel).Use(sink));

        var msProvider = provider.Resolve<IMicrosoftLoggerProvider>();
        var logger = msProvider.CreateLogger("test-source");

        return (logger, sink);
    }
}
