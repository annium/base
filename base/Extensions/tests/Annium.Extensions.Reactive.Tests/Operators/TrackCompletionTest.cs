using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Reactive.Operators;
using Annium.Logging;
using Annium.Testing;
using Xunit;

namespace Annium.Extensions.Reactive.Tests.Operators;

/// <summary>
/// Tests for the TrackCompletion operator in reactive extensions.
/// </summary>
public class TrackCompletionTest : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TrackCompletionTest"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging test results.</param>
    public TrackCompletionTest(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Tests that the TrackCompletion operator works correctly with incomplete observables,
    /// properly tracking completion state.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task TrackCompletion_IncompleteWorks()
    {
        // arrange
        var logger = Get<ILogger>();
        var cts = new CancellationTokenSource();
        var observable = ObservableExt
            .StaticAsyncInstance<string>(
                async ctx =>
                {
                    await Task.Delay(10, ctx.Ct);

                    return () => Task.CompletedTask;
                },
                cts.Token,
                logger
            )
            .TrackCompletion(logger);

        // act
        await observable.WhenCompletedAsync(logger);
    }

    /// <summary>
    /// Tests that the TrackCompletion operator works correctly with complete observables,
    /// properly tracking completion state.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task TrackCompletion_CompleteWorks()
    {
        // arrange
        var logger = Get<ILogger>();
        var cts = new CancellationTokenSource();
        var observable = ObservableExt
            .StaticAsyncInstance<string>(
                async _ =>
                {
                    await Task.Delay(10, CancellationToken.None);
                    return () => Task.CompletedTask;
                },
                cts.Token,
                logger
            )
            .TrackCompletion(logger);

        // act
        await observable.WhenCompletedAsync(logger);
    }
}
