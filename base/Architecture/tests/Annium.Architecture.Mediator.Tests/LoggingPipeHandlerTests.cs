using System.Linq;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Core.Mediator;
using Annium.Data.Operations;
using Annium.Logging;
using Annium.Testing;
using Annium.Threading.Tasks;
using Xunit;

namespace Annium.Architecture.Mediator.Tests;

/// <summary>
/// Tests for the logging pipe handler functionality.
/// </summary>
[Collection("LogConfigMutating")]
public class LoggingPipeHandlerTests : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LoggingPipeHandlerTests"/> class.
    /// </summary>
    /// <param name="outputHelper">xUnit test output helper the test host logs through.</param>
    public LoggingPipeHandlerTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        // Trace level is required so the LoggingPipeHandler's this.Trace(...) start/complete entries
        // are captured in Logs. OverrideLogLevel snapshots the prior level and restores it via
        // TestBase.DisposeAsync; the [Collection] attribute above serialises this class with the
        // other LogConfig-mutating test class so their global mutations don't race under xunit
        // parallel-class execution.
        OverrideLogLevel(LogLevel.Trace);

        RegisterMediator(cfg => cfg.AddLoggingHandler().AddHandler(typeof(EchoRequestHandler<>)));
    }

    /// <summary>
    /// Tests that the logging handler returns the original result.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task ReturnsOriginalResult()
    {
        // arrange
        var mediator = Get<IMediator>();
        var request = new ThrowingRequest();

        // act
        var result = await mediator.SendAsync<IStatusResult<OperationStatus, ThrowingRequest>>(
            request,
            TestContext.Current.CancellationToken
        );

        // assert
        result.Status.Is(OperationStatus.Ok);
        result.IsOk.IsTrue();
    }

    /// <summary>
    /// Tests that the logging handler emits a Trace entry for request start and a Trace entry for
    /// request completion. Verifies that removing either <c>this.Trace(...)</c> call in
    /// <c>LoggingPipeHandler</c> would cause this test to fail.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task SendAsync_WithLoggingHandler_EmitsStartAndCompleteTraceEntries()
    {
        // arrange
        var mediator = Get<IMediator>();
        var request = new ThrowingRequest();

        // act
        await mediator.SendAsync<IStatusResult<OperationStatus, ThrowingRequest>>(
            request,
            TestContext.Current.CancellationToken
        );

        // Wait for the async log dispatcher to flush both Trace entries.
        await Wait.UntilAsync(
            () => Logs.Count(m => m.Level == LogLevel.Trace) >= 2,
            TestContext.Current.CancellationToken
        );

        // assert — filter to Trace entries only and verify both message templates are present
        var traceEntries = Logs.Where(m => m.Level == LogLevel.Trace).ToList();

        traceEntries.Any(m => m.MessageTemplate == "Start {request} -> {response}").IsTrue();
        traceEntries.Any(m => m.MessageTemplate == "Complete {request} -> {response}").IsTrue();
    }
}
