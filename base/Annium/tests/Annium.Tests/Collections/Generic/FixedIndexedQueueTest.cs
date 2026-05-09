using System;
using System.Linq;
using Annium.Collections.Generic;
using Annium.Testing;
using Xunit;

namespace Annium.Tests.Collections.Generic;

/// <summary>
/// Contains unit tests for <see cref="FixedIndexedQueue{T}"/> to verify fixed-size queue behavior.
/// </summary>
public class FixedIndexedQueueTest
{
    /// <summary>
    /// Verifies that adding elements, counting, indexing, and enumeration work correctly.
    /// </summary>
    [Fact]
    public void Add_Count_Index_Enumerate()
    {
        // arrange
        var queue = new FixedIndexedQueue<int>(3);

        // act & assert
        queue.Capacity.Is(3);

        // initial fill
        queue.Add(1);
        queue.Count.Is(1);
        queue[0].Is(1);

        queue.Add(2);
        queue.Count.Is(2);
        queue[0].Is(1);
        queue[1].Is(2);

        queue.Add(3);
        queue.Count.Is(3);
        queue[0].Is(1);
        queue[1].Is(2);
        queue[2].Is(3);

        queue.Add(4);
        queue.Count.Is(3);
        queue[0].Is(2);
        queue[1].Is(3);
        queue[2].Is(4);

        queue.Add(5);
        queue.Count.Is(3);
        queue[0].Is(3);
        queue[1].Is(4);
        queue[2].Is(5);

        queue.Add(6);
        queue.Count.Is(3);
        queue[0].Is(4);
        queue[1].Is(5);
        queue[2].Is(6);

        queue.Add(7);
        queue.Count.Is(3);
        queue[0].Is(5);
        queue[1].Is(6);
        queue[2].Is(7);

        var list = queue.ToArray();
        list.IsEqual(new[] { 5, 6, 7 });
    }

    /// <summary>
    /// Verifies that the indexer guards against reads past <c>Count</c> while the queue is partially filled.
    /// Without the guard, the indexer would return <c>default(T)</c> from uninitialized backing slots.
    /// </summary>
    [Fact]
    public void Indexer_PartiallyFilled_ThrowsAtCount()
    {
        // arrange — capacity 3, only 1 element added
        var queue = new FixedIndexedQueue<int>(3);
        queue.Add(42);

        // assert
        queue.Count.Is(1);
        queue[0].Is(42);
        Wrap.It(() =>
            {
                _ = queue[1];
            })
            .Throws<ArgumentOutOfRangeException>();
        Wrap.It(() =>
            {
                _ = queue[2];
            })
            .Throws<ArgumentOutOfRangeException>();
        Wrap.It(() =>
            {
                _ = queue[-1];
            })
            .Throws<ArgumentOutOfRangeException>();
    }

    /// <summary>
    /// Verifies that creating a queue from an existing collection works correctly.
    /// </summary>
    [Fact]
    public void Create()
    {
        // arrange
        var queue = new FixedIndexedQueue<int>(new[] { 1, 2 });

        // act & assert
        queue.Capacity.Is(2);
        queue.Count.Is(2);
        queue[0].Is(1);
        queue[1].Is(2);
    }
}
