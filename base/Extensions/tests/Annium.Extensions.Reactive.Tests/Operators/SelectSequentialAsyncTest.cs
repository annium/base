using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Annium.Testing;
using Xunit;

namespace Annium.Extensions.Reactive.Tests.Operators;

public class SelectSequentialAsyncTest
{
    [Fact]
    public async Task SelectSequentialAsync_WorksCorrectly()
    {
        // arrange
        var log = new List<string>();
        var tcs = new TaskCompletionSource();
        using var observable = Observable.Range(1, 5)
            .SelectSequentialAsync(async x =>
            {
                lock (log)
                    log.Add($"start: {x}");
                await Task.Delay(10);
                lock (log)
                    log.Add($"end: {x}");
                return x;
            })
            .Subscribe(x =>
            {
                lock (log)
                    log.Add($"sub: {x}");
            }, tcs.SetResult);

        await tcs.Task;

        log.Has(15);
        var expectedLog = Enumerable.Range(1, 5)
            .Select(x => new[]
            {
                $"start: {x}",
                $"end: {x}",
                $"sub: {x}"
            })
            .SelectMany(x => x)
            .ToArray();
        log.IsEqual(expectedLog);
    }
}