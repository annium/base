using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Annium.Extensions.Reactive.Operators;
using Annium.Testing;
using Xunit;

namespace Annium.Extensions.Reactive.Tests.Operators;

/// <summary>
/// Tests for the DoParallelAsync operator in reactive extensions.
/// </summary>
public class DoParallelAsyncTest : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DoParallelAsyncTest"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging test results.</param>
    public DoParallelAsyncTest(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        RegisterTestLogs();
    }

    /// <summary>
    /// Tests that the DoParallelAsync operator executes side effects in parallel,
    /// allowing concurrent execution of async operations.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task DoParallelAsync_WorksCorrectly()
    {
        // arrange
        var log = Get<TestLog<string>>();
        var tcs = new TaskCompletionSource();
        using var observable = Observable
            .Range(1, 100)
            .DoParallelAsync(async x =>
            {
                log.Add($"start: {x}");
                await Task.Delay(100);
                log.Add($"end: {x}");
            })
            .Subscribe(_ => { }, tcs.SetResult);

        await tcs.Task;

        log.Has(200);
        var starts = log.Select((x, i) => (x, i)).Where(x => x.x.StartsWith("start:")).Select(x => x.i).ToArray();
        var ends = log.Select((x, i) => (x, i)).Where(x => x.x.StartsWith("end:")).Select(x => x.i).ToArray();

        // at least one start/end pair will have sequential position in log
        starts.Any(x => starts.Contains(x - 1)).IsTrue();
        ends.Any(x => ends.Contains(x - 1)).IsTrue();
    }
}
