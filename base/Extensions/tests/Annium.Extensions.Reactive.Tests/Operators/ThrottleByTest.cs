using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Annium.Testing;
using NodaTime;
using Xunit;

namespace Annium.Extensions.Reactive.Tests.Operators;

/// <summary>
/// Tests for the ThrottleBy operator: throttling is per key, so a busy key must not silence a quiet one.
/// </summary>
public class ThrottleByTest
{
    /// <summary>
    /// Within one window a key emits once, while a different key passes independently.
    /// </summary>
    [Fact]
    public void ThrottleBy_SameKeyWithinInterval_EmitsOnce()
    {
        // arrange
        var received = new List<string>();
        var subject = new Subject<string>();
        using var subscription = subject
            .ThrottleBy(x => x[..1], Duration.FromSeconds(30))
            .Subscribe(x => received.Add(x));

        // act - three values sharing a key, one with its own
        subject.OnNext("a1");
        subject.OnNext("a2");
        subject.OnNext("b1");
        subject.OnNext("a3");

        // assert - the first of each key gets through, the rest of "a" is throttled
        received.Has(2).At(0).Is("a1");
        received.At(1).Is("b1");
    }

    /// <summary>
    /// Once the window has passed the key emits again.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task ThrottleBy_AfterInterval_EmitsAgain()
    {
        // arrange - a window short enough to wait out
        var received = new List<string>();
        var subject = new Subject<string>();
        using var subscription = subject
            .ThrottleBy(x => x[..1], Duration.FromMilliseconds(50))
            .Subscribe(x => received.Add(x));

        // act
        subject.OnNext("a1");
        await Task.Delay(TimeSpan.FromMilliseconds(150), TestContext.Current.CancellationToken);
        subject.OnNext("a2");

        // assert
        received.Has(2).At(1).Is("a2");
    }

    /// <summary>
    /// Completion travels through the operator, so a consumer waiting on it is not left hanging.
    /// </summary>
    [Fact]
    public void ThrottleBy_SourceCompletes_CompletionPropagates()
    {
        // arrange
        var completed = false;
        var subject = new Subject<string>();
        using var subscription = subject
            .ThrottleBy(x => x, Duration.FromSeconds(30))
            .Subscribe(_ => { }, () => completed = true);

        // act
        subject.OnCompleted();

        // assert
        completed.IsTrue("completion must reach the subscriber");
    }
}
