using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Testing;
using Annium.Testing.Lib;
using Xunit;
using Xunit.Abstractions;

namespace Annium.Extensions.Reactive.Tests.Creation.ObservableInstance;

public class StaticObservableInstanceTest : TestBase
{
    public StaticObservableInstanceTest(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public async Task Events_AreEmittedCorrectly()
    {
        // arrange
        var log1 = new List<Sample>();
        var log2 = new List<Sample>();
        var errors = new List<Exception>();
        var disposeCounter = 0;
        var instance = ObservableExt
            .StaticAsyncInstance<Sample>(async ctx =>
            {
                for (int i = 0; i < 5; i++)
                {
                    await Task.Delay(100);
                    ctx.OnNext(new Sample(i));
                    if (i == 2)
                        ctx.OnError(new ArgumentOutOfRangeException(nameof(i)));
                }

                return async () =>
                {
                    await Task.Delay(5);
                    Interlocked.Increment(ref disposeCounter);
                };
            }, CancellationToken.None, Get<ILogger>())
            .Do(_ => { }, errors.Add)
            .Retry()
            .Catch(Observable.Empty<Sample>());
        instance.Subscribe(log1.Add);
        instance.Subscribe(log2.Add);

        await instance;

        log1.Has(5);
        log2.Has(5);
        for (var i = 0; i < log1.Count; i++)
            log1[i].Is(log2[i]);
        errors.Has(3);
        var error = errors[0];
        foreach (var err in errors.Skip(1))
            err.Is(error);
        disposeCounter.Is(1);
    }

    private class Sample
    {
        public int Value { get; }

        public Sample(int value)
        {
            Value = value;
        }

        public override string ToString() => Value.ToString();
    }
}