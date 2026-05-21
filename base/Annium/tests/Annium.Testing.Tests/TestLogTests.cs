using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Annium.Testing.Tests;

/// <summary>
/// Tests for <see cref="TestLog{T}"/>. Uses xunit-native assertions to avoid circular
/// dependency on Annium.Testing.
/// </summary>
public class TestLogTests
{
    /// <summary>Verifies Add increases the Count.</summary>
    [Fact]
    public void Add_IncreasesCount()
    {
        var log = new TestLog<string>();

        log.Add("a");
        log.Add("b");

        Assert.Equal(2, log.Count);
    }

    /// <summary>Verifies Clear resets the Count to zero.</summary>
    [Fact]
    public void Clear_ResetsCount()
    {
        var log = new TestLog<string>();
        log.Add("a");
        log.Add("b");

        log.Clear();

        Assert.Empty(log);
    }

    /// <summary>Verifies the indexer returns entries in insertion order.</summary>
    [Fact]
    public void Indexer_ReturnsCorrectItem()
    {
        var log = new TestLog<int>();
        log.Add(10);
        log.Add(20);
        log.Add(30);

        Assert.Equal(10, log[0]);
        Assert.Equal(20, log[1]);
        Assert.Equal(30, log[2]);
    }

    /// <summary>Verifies GetEnumerator yields all entries in order.</summary>
    [Fact]
    public void GetEnumerator_YieldsAllItems()
    {
        var log = new TestLog<int>();
        log.Add(1);
        log.Add(2);
        log.Add(3);

        var items = log.ToList();

        Assert.Equal([1, 2, 3], items);
    }

    /// <summary>
    /// Verifies that <see cref="TestLog{T}.GetEnumerator"/> returns an enumerator over an
    /// independent copy, not a live reference into the internal list. The enumerator is
    /// captured BEFORE the log is mutated, then iterated AFTER. With a correct copy-based
    /// implementation the iteration yields the pre-mutation snapshot; with a broken live-ref
    /// implementation, <see cref="List{T}.Enumerator"/> would throw
    /// <see cref="InvalidOperationException"/> on the first <c>MoveNext</c> after the version
    /// counter changed (or, less commonly, yield the post-mutation contents). Single-threaded
    /// and deterministic — no timing tricks needed.
    /// </summary>
    [Fact]
    public void GetEnumerator_ReturnsSnapshotNotLiveReference()
    {
        var log = new TestLog<int>();
        log.Add(1);
        log.Add(2);
        log.Add(3);

        var enumerator = log.GetEnumerator();
        log.Add(4);
        log.Add(5);

        var items = new List<int>();
        while (enumerator.MoveNext())
            items.Add(enumerator.Current);

        Assert.Equal([1, 2, 3], items);
        Assert.Equal(5, log.Count);
    }

    /// <summary>
    /// Verifies the non-generic <see cref="IEnumerable.GetEnumerator"/> overload yields all entries
    /// in insertion order under lock. Closes the TG1 coverage gap from review-7 — the non-generic
    /// overload had zero direct test hits.
    /// </summary>
    [Fact]
    public void NonGenericEnumerator_YieldsAllItems()
    {
        var log = new TestLog<int>();
        log.Add(1);
        log.Add(2);
        log.Add(3);

        var items = new List<int>();
        var enumerator = ((IEnumerable)log).GetEnumerator();
        while (enumerator.MoveNext())
            items.Add((int)enumerator.Current!);

        Assert.Equal([1, 2, 3], items);
    }

    /// <summary>
    /// Verifies that enumeration is unaffected by concurrent Add calls — the snapshot taken
    /// under the lock in GetEnumerator must not throw, must remain stable across re-enumeration,
    /// and the snapshot count must be monotonic across iterations. Closes the D5 lock-escape
    /// regression class plus the residual vacuous-pass paths flagged as B1 + TG2 in review-7
    /// (broad OperationCanceledException catch silenced TestContext-driven cancellation; the
    /// >= 50 floor was satisfied by the pre-seed even with zero writer activity; snapshot
    /// stability was unverified).
    /// </summary>
    [Fact]
    public async Task GetEnumerator_ConcurrentAdds_DoesNotThrow()
    {
        var log = new TestLog<int>();
        for (var i = 0; i < 50; i++)
            log.Add(i);

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(250));

        var writer = Task.Run(
            () =>
            {
                var n = 100;
                while (!cts.Token.IsCancellationRequested)
                {
                    log.Add(n++);
                }
            },
            TestContext.Current.CancellationToken
        );

        var iterations = 0;
        var prevCount = 0;
        for (var i = 0; i < 200 && !cts.Token.IsCancellationRequested; i++)
        {
            iterations++;
            var snapshot = log.ToList();
            Assert.True(snapshot.Count >= prevCount, "snapshot count must be monotonic across iterations");
            prevCount = snapshot.Count;
        }
        Assert.True(iterations > 0, "snapshot loop never executed — test had no coverage");

        try
        {
            await writer;
        }
        catch (OperationCanceledException) when (cts.IsCancellationRequested)
        {
            // expected — writer was cancelled by cts; non-cts cancellation propagates as a test failure
        }

        // Post-race assertion: at least one writer add must have landed.
        // Proves the concurrent-add path was actually exercised, not just the pre-seed reads.
        Assert.True(log.Count > 50, "no writer adds landed — concurrent-add path was not exercised");
    }

    /// <summary>
    /// Verifies that concurrent writers from multiple threads each contribute their full disjoint
    /// slice with no dropped writes under contention. Closes the TG3 writer-correctness gap from
    /// review-7 — the prior concurrent test only proved snapshot reads don't throw, never that
    /// concurrent writes actually land.
    /// </summary>
    [Fact]
    public async Task ConcurrentAdds_AllItemsPresent()
    {
        var log = new TestLog<int>();
        const int writers = 8;
        const int perWriter = 1000;
        var ct = TestContext.Current.CancellationToken;

        var tasks = Enumerable
            .Range(0, writers)
            .Select(w =>
                Task.Run(
                    () =>
                    {
                        for (var i = 0; i < perWriter; i++)
                            log.Add(w * perWriter + i);
                    },
                    ct
                )
            )
            .ToArray();

        await Task.WhenAll(tasks);

        Assert.Equal(writers * perWriter, log.Count);

        var sorted = log.OrderBy(x => x).ToList();
        var expected = Enumerable.Range(0, writers * perWriter).ToList();
        Assert.Equal(expected, sorted);
    }

    /// <summary>Verifies a fresh log starts with Count = 0 and yields nothing.</summary>
    [Fact]
    public void Empty_HasZeroCountAndEmptyEnumeration()
    {
        var log = new TestLog<string>();

        Assert.Empty(log);
    }
}
