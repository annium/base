using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Annium.Testing;
using Xunit;

namespace Annium.Extensions.Reactive.Tests.Operators;

/// <summary>
/// Tests for the SubscribeAsync operator in reactive extensions.
/// </summary>
public class SubscribeAsyncTest : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SubscribeAsyncTest"/> class.
    /// </summary>
    /// <param name="outputHelper">xUnit test output helper the test host logs through.</param>
    public SubscribeAsyncTest(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Tests that the SubscribeAsync operator correctly handles errors asynchronously
    /// when subscribing to an observable sequence.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task SubscribeAsync_OnErrorWorksCorrectly()
    {
        // arrange
        var log = new TestLog<string>();
        var tcs = new TaskCompletionSource();
        await using var observable = Observable
            .Range(1, 5)
            .Select(x =>
            {
                if (x == 3)
                    throw new InvalidOperationException("3");

                lock (log)
                    log.Add($"add: {x}");

                return x;
            })
            .SubscribeAsync(
                async e =>
                {
                    await Task.Delay(10);
                    lock (log)
                        log.Add($"err: {e.Message}");
                    await Task.Delay(10);
                    tcs.SetResult();
                },
                Logger
            );

        await Bounded.AwaitAsync(tcs.Task);

        log.Has(3);
        log[2].Is("err: 3");
    }

    /// <summary>
    /// Tests that the SubscribeAsync operator correctly handles completion asynchronously
    /// when subscribing to an observable sequence.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task SubscribeAsync_OnCompletedWorksCorrectly()
    {
        // arrange
        var log = new TestLog<string>();
        var tcs = new TaskCompletionSource();
        await using var observable = Observable
            .Range(1, 5)
            .Select(x =>
            {
                lock (log)
                    log.Add($"add: {x}");

                return x;
            })
            .SubscribeAsync(
                async () =>
                {
                    await Task.Delay(10);
                    lock (log)
                        log.Add("done");
                    tcs.SetResult();
                },
                Logger
            );

        await Bounded.AwaitAsync(tcs.Task);

        log.Has(6);
        log[5].Is("done");
    }

    /// <summary>
    /// A handler that throws is reported. The subscription runs its handlers on a background executor that
    /// catches and logs; with no logger to catch it, such a failure used to vanish completely.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task SubscribeAsync_HandlerThrows_IsLogged()
    {
        // arrange
        var tcs = new TaskCompletionSource();

        // act
        await using var observable = Observable
            .Range(1, 1)
            .SubscribeAsync(
                // typed explicitly: an untyped lambda matches both the onNext and onError overloads
                async (int _) =>
                {
                    await Task.Delay(10);
                    tcs.TrySetResult();

                    throw new InvalidOperationException("handler blew up");
                },
                Logger
            );
        await Bounded.AwaitAsync(tcs.Task);

        // assert
        await Expect.ToAsync(() =>
            Logs.Any(x => x.Message.Contains("handler blew up")).IsTrue("a failing handler must be reported")
        );
    }
}
