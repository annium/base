using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Testing.Lib;
using Xunit;
using Xunit.Abstractions;

namespace Annium.Extensions.Reactive.Tests.Operators;

public class TrackCompletionTest : TestBase
{
    public TrackCompletionTest(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public async Task TrackCompletion_IncompleteWorks()
    {
        // arrange
        var logger = Get<ILogger>();
        var cts = new CancellationTokenSource();
        var observable = ObservableExt.StaticAsyncInstance<string>(async ctx =>
        {
            await Task.Delay(10, ctx.Ct);

            return () => Task.CompletedTask;
        }, cts.Token, logger).TrackCompletion(logger);

        // act
        await observable.WhenCompleted(logger);
    }

    [Fact]
    public async Task TrackCompletion_CompleteWorks()
    {
        // arrange
        var logger = Get<ILogger>();
        var cts = new CancellationTokenSource();
        var observable = ObservableExt.StaticAsyncInstance<string>(async _ =>
        {
            await Task.Delay(10, CancellationToken.None);
            return () => Task.CompletedTask;
        }, cts.Token, logger).TrackCompletion(logger);

        // act
        await observable.WhenCompleted(logger);
    }
}