using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Testing;
using Annium.Testing.Lib;
using Xunit;
using Xunit.Abstractions;

namespace Annium.Extensions.Reactive.Tests.Operators;

public class WhenCompletedTest : TestBase
{
    public WhenCompletedTest(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public async Task SubscribeAsync_OnErrorWorksCorrectly()
    {
        // arrange
        var log = new List<long>();

        // act
        await Observable.Interval(TimeSpan.FromMilliseconds(20)).Take(5).Do(log.Add).WhenCompleted(Get<ILogger>());

        log.IsEqual(new[] { 0, 1, 2, 3, 4 });
    }
}