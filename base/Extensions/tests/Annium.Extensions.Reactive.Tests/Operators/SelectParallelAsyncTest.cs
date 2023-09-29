using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Annium.Testing;
using Annium.Testing.Lib;
using Xunit;
using Xunit.Abstractions;

namespace Annium.Extensions.Reactive.Tests.Operators;

public class SelectParallelAsyncTest : TestBase
{
    public SelectParallelAsyncTest(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
    }

    [Fact]
    public async Task SelectParallelAsync_WorksCorrectly()
    {
        // arrange
        var log = new ConcurrentQueue<string>();
        var tcs = new TaskCompletionSource();
        using var observable = Observable.Range(1, 5)
            .SelectParallelAsync(async x =>
            {
                log.Enqueue($"start: {x}");
                await Task.Delay(100);
                log.Enqueue($"end: {x}");
                return x;
            })
            .Subscribe(_ => { }, tcs.SetResult);

        await tcs.Task;

        log.Has(10);
        var starts = log.Select((x, i) => (x, i)).Where(x => x.x.StartsWith("start:")).Select(x => x.i).ToArray();
        var ends = log.Select((x, i) => (x, i)).Where(x => x.x.StartsWith("end:")).Select(x => x.i).ToArray();

        // at least one start/end pair will have sequential position in log
        starts.Any(x => starts.Contains(x - 1)).IsTrue();
        ends.Any(x => ends.Contains(x - 1)).IsTrue();
    }
}