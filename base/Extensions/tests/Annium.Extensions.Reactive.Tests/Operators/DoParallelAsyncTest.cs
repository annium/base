using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Annium.Testing;
using Xunit;

namespace Annium.Extensions.Reactive.Tests.Operators;

public class DoParallelAsyncTest : TestBase
{
    public DoParallelAsyncTest(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        RegisterTestLogs();
    }

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
