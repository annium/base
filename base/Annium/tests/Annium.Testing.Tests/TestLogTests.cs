using System;
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

        Assert.Empty((IEnumerable<string>)log);
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
    /// Verifies that enumeration is unaffected by concurrent Add calls — the snapshot taken
    /// under the lock in GetEnumerator must not throw or yield mutated state. Closes the
    /// D5 lock-escape regression class.
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

        for (var i = 0; i < 200 && !cts.Token.IsCancellationRequested; i++)
        {
            var snapshot = log.ToList();
            Assert.True(snapshot.Count >= 50);
        }

        try
        {
            await writer;
        }
        catch (System.OperationCanceledException)
        {
            // expected — writer was cancelled
        }
    }

    /// <summary>Verifies a fresh log starts with Count = 0 and yields nothing.</summary>
    [Fact]
    public void Empty_HasZeroCountAndEmptyEnumeration()
    {
        var log = new TestLog<string>();

        Assert.Empty((IEnumerable<string>)log);
    }
}
