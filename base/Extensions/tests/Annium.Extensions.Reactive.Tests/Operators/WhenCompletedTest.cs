using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Testing;
using Xunit;

namespace Annium.Extensions.Reactive.Tests.Operators;

/// <summary>
/// Tests for the WhenCompleted operator in reactive extensions.
/// </summary>
public class WhenCompletedTest : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WhenCompletedTest"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging test results.</param>
    public WhenCompletedTest(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Tests that the WhenCompletedAsync operator correctly waits for observable completion
    /// and processes all emitted values.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task SubscribeAsync_OnErrorWorksCorrectly()
    {
        // arrange
        var log = new TestLog<long>();

        // act
        await Observable.Interval(TimeSpan.FromMilliseconds(20)).Take(5).Do(log.Add).WhenCompletedAsync(Get<ILogger>());

        log.IsEqual(new[] { 0, 1, 2, 3, 4 });
    }
}
