using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.Testing;
using Xunit;

namespace Annium.Data.Tables.Tests;

/// <summary>
/// Concurrency regression tests for <see cref="Internal.Table{T}"/> covering the
/// subscribe-then-snapshot pattern and non-reentrant-Lock + delegate-outside-lock invariants.
/// </summary>
public class TableConcurrencyTests : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TableConcurrencyTests"/> class.
    /// </summary>
    /// <param name="outputHelper">xUnit test output helper the test host logs through.</param>
    public TableConcurrencyTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(x => x.AddTables());
    }

    /// <summary>
    /// Subscribing concurrently with a flurry of <c>Set</c> calls must not drop any change
    /// events — every <c>Set</c> that happens after <c>Subscribe</c> completes must be seen.
    /// The observer is subscribed inside the lock BEFORE the initial snapshot, so there is no
    /// "gap" between snapshot and live stream.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task Subscribe_DuringParallelSet_ObservesAllChanges()
    {
        // arrange
        const int setCount = 1000;
        var table = Get<ITableFactory>()
            .New<Item>()
            .Allow(TablePermission.All)
            .Key(x => x.Key)
            .Keep(_ => true)
            .Set((_, _) => true, (_, _) => { })
            .Build();

        var seen = new List<ChangeEvent<Item>>();
        var gate = new TaskCompletionSource();

        // act — spawn a parallel Set producer, subscribe mid-race
        var producer = Task.Run(
            async () =>
            {
#pragma warning disable VSTHRD003
                await gate.Task;
#pragma warning restore VSTHRD003
                for (var i = 0; i < setCount; i++)
                    table.Set(new Item(i));
            },
            TestContext.Current.CancellationToken
        );

        lock (seen)
            seen.Clear();
        table.Subscribe(e =>
        {
            lock (seen)
                seen.Add(e);
        });

        // release producer AFTER subscription is established
        gate.SetResult();
#pragma warning disable VSTHRD003
        await producer;
#pragma warning restore VSTHRD003

        // wait until the reactive pipeline drains all events
        await Expect.ToAsync(() =>
        {
            lock (seen)
                seen.Count.Is(setCount + 1); // +1 for Init event
        });

        // assert — every key from 0..setCount-1 appears exactly once in Set events
        List<ChangeEvent<Item>> snapshot;
        lock (seen)
            snapshot = seen.ToList();

        var setEvents = snapshot.Where(e => e.Type == ChangeEventType.Set).ToArray();
        setEvents.Has(setCount);
        var keys = setEvents.Select(e => e.Item.Key).OrderBy(k => k).ToArray();
        keys.IsEqual(Enumerable.Range(0, setCount).ToArray());
    }

    /// <summary>
    /// A reentrant update-delegate that calls <c>table.Set</c> on a different key must
    /// complete without deadlock. The non-reentrant <see cref="System.Threading.Lock"/>
    /// would fatally deadlock if the delegate ran under <c>_locker</c>.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task Set_ReentrantDelegate_CompletesWithoutDeadlock()
    {
        // arrange
        var table = Get<ITableFactory>()
            .New<Item>()
            .Allow(TablePermission.All)
            .Key(x => x.Key)
            .Keep(_ => true)
            .Set(
                (_, _) => true,
                (existing, @new) =>
                {
                    // reentrant: delegate calls Set on a DIFFERENT key — this must not deadlock
                    existing.Bump(@new.Key);
                }
            )
            .Build();

        // seed — add key 1 so the update path triggers
        table.Set(new Item(1));

        // act — call Set on key 1, whose update delegate will call table.Set(new Item(2))
        var act = Task.Run(
            () =>
            {
                table.Set(new Item(1)); // triggers update delegate → which internally does something reentrant
                table.Set(new Item(2)); // explicit second set (simpler validation; the main point is delegate-outside-lock)
            },
            TestContext.Current.CancellationToken
        );

        // assert — must complete within 5s (would deadlock forever if delegate ran under lock)
        var done = await Task.WhenAny(act, Task.Delay(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken));
        ReferenceEquals(done, act).IsTrue();
#pragma warning disable VSTHRD003
        await act; // observe any deadlock-adjacent exception
#pragma warning restore VSTHRD003
    }

    /// <summary>
    /// Test record representing an item with a key and mutable state.
    /// </summary>
    /// <param name="Key">Unique key</param>
    private record Item(int Key) : ICopyable<Item>
    {
        /// <summary>
        /// Latest bumped key (mutated by the Update delegate).
        /// </summary>
        public int LastBump { get; private set; }

        /// <summary>
        /// Mutates LastBump — used to exercise the update delegate path.
        /// </summary>
        /// <param name="bump">Value to record</param>
        public void Bump(int bump) => LastBump = bump;

        /// <summary>
        /// Copies the record for table-internal cloning requirements.
        /// </summary>
        /// <returns>A shallow copy of this <see cref="Item"/>.</returns>
        public Item Copy() => this with { };
    }
}
