using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Testing;
using Xunit;

namespace Annium.Extensions.Reactive.Tests.Creation.ObservableInstance;

/// <summary>
/// Tests for the StaticObservableInstance functionality in the reactive extensions.
/// </summary>
public class StaticObservableInstanceTest : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StaticObservableInstanceTest"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging test results.</param>
    public StaticObservableInstanceTest(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Tests that events are emitted correctly from a static observable instance,
    /// including proper retry behavior and error handling.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Events_AreEmittedCorrectly()
    {
        // arrange
        var log1 = new List<Sample>();
        var log2 = new List<Sample>();
        var errors = new List<Exception>();
        var disposeCounter = 0;
        var instance = ObservableExt
            .StaticAsyncInstance<Sample>(
                async ctx =>
                {
                    for (var i = 0; i < 5; i++)
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
                },
                CancellationToken.None,
                Get<ILogger>()
            )
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

    /// <summary>
    /// A sample data class used for testing observable events.
    /// </summary>
    private class Sample
    {
        /// <summary>
        /// Gets the integer value of this sample.
        /// </summary>
        public int Value { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Sample"/> class.
        /// </summary>
        /// <param name="value">The integer value for this sample.</param>
        public Sample(int value)
        {
            Value = value;
        }

        /// <summary>
        /// Returns a string representation of this sample.
        /// </summary>
        /// <returns>The string representation of the sample value.</returns>
        public override string ToString() => Value.ToString();
    }
}
