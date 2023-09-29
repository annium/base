using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Annium.Testing;
using Xunit;

namespace Annium.Extensions.Reactive.Tests.Operators;

public class CatchAsyncTest
{
    [Fact]
    public async Task CatchAsync_WorksCorrectly()
    {
        // arrange
        var log = new List<string>();
        var tcs = new TaskCompletionSource();
        using var observable = Observable.Range(1, 5)
            .Select(x =>
            {
                if (x == 3)
                    throw new InvalidOperationException("3");

                lock (log)
                    log.Add($"add: {x}");

                return x;
            })
            .CatchAsync(async (InvalidOperationException e) =>
            {
                await Task.Delay(10);
                lock (log)
                    log.Add($"err: {e.Message}");

                return Observable.Empty<int>();
            })
            .Subscribe(x =>
            {
                lock (log)
                    log.Add($"sub: {x}");
            }, () =>
            {
                lock (log)
                    log.Add("done");
                tcs.SetResult();
            });

        await tcs.Task;

        log.Has(6);
        log[4].Is("err: 3");
        log[5].Is("done");
    }
}