using System;
using System.Linq;
using Annium.Collections.Generic;
using Annium.Testing;
using Xunit;

namespace Annium.Tests.Collections.Generic;

/// <summary>
/// Contains unit tests for <see cref="DoubleEdgeQueue{T}"/> to verify queue operations in both direct and reverse modes.
/// </summary>
public class DoubleEdgeQueueTest
{
    /// <summary>
    /// Verifies that adding elements to the first position works correctly in direct mode.
    /// </summary>
    [Fact]
    public void Direct_AddFirst()
    {
        // arrange
        var queue = new DoubleEdgeQueue<int>(true);

        // act & assert
        queue.AddFirst(1);
        queue.Has(1);
        queue.First.Is(1);
        queue.Last.Is(1);
        queue.AddFirst(2);
        queue.Has(2);
        queue.First.Is(2);
        queue.Last.Is(1);
        queue.ToArray().IsEqual(new[] { 2, 1 });
    }

    /// <summary>
    /// Verifies that adding elements to the last position works correctly in direct mode.
    /// </summary>
    [Fact]
    public void Direct_AddLast()
    {
        // arrange
        var queue = new DoubleEdgeQueue<int>(true);

        // act & assert
        queue.AddLast(1);
        queue.Has(1);
        queue.First.Is(1);
        queue.Last.Is(1);
        queue.AddLast(2);
        queue.Has(2);
        queue.First.Is(1);
        queue.Last.Is(2);
        queue.ToArray().IsEqual(new[] { 1, 2 });
    }

    /// <summary>
    /// Verifies that removing elements from the first position works correctly in direct mode.
    /// </summary>
    [Fact]
    public void Direct_RemoveFirst()
    {
        // arrange
        var queue = new DoubleEdgeQueue<int>(true);
        queue.AddFirst(1);
        queue.AddFirst(2);

        // act & assert
        queue.RemoveFirst();
        queue.Has(1);
        queue.First.Is(1);
        queue.RemoveFirst();
        queue.IsEmpty();
        Wrap.It(() => queue.RemoveFirst()).Throws<InvalidOperationException>();
    }

    /// <summary>
    /// Verifies that removing elements from the last position works correctly in direct mode.
    /// </summary>
    [Fact]
    public void Direct_RemoveLast()
    {
        // arrange
        var queue = new DoubleEdgeQueue<int>(true);
        queue.AddFirst(1);
        queue.AddFirst(2);

        // act & assert
        queue.RemoveLast();
        queue.Has(1);
        queue.First.Is(2);
        queue.RemoveLast();
        queue.IsEmpty();
        Wrap.It(() => queue.RemoveLast()).Throws<InvalidOperationException>();
    }

    /// <summary>
    /// Verifies that adding elements to the first position works correctly in reverse mode.
    /// </summary>
    [Fact]
    public void Reverse_AddFirst()
    {
        // arrange
        var queue = new DoubleEdgeQueue<int>(false);

        // act & assert
        queue.AddFirst(1);
        queue.Has(1);
        queue.First.Is(1);
        queue.Last.Is(1);
        queue.AddFirst(2);
        queue.Has(2);
        queue.First.Is(1);
        queue.Last.Is(2);
        queue.ToArray().IsEqual(new[] { 1, 2 });
    }

    /// <summary>
    /// Verifies that adding elements to the last position works correctly in reverse mode.
    /// </summary>
    [Fact]
    public void Reverse_AddLast()
    {
        // arrange
        var queue = new DoubleEdgeQueue<int>(false);

        // act & assert
        queue.AddLast(1);
        queue.Has(1);
        queue.First.Is(1);
        queue.Last.Is(1);
        queue.AddLast(2);
        queue.Has(2);
        queue.First.Is(2);
        queue.Last.Is(1);
        queue.ToArray().IsEqual(new[] { 2, 1 });
    }

    /// <summary>
    /// Verifies that removing elements from the first position works correctly in reverse mode.
    /// </summary>
    [Fact]
    public void Reverse_RemoveFirst()
    {
        // arrange
        var queue = new DoubleEdgeQueue<int>(false);
        queue.AddFirst(1);
        queue.AddFirst(2);

        // act & assert
        queue.RemoveFirst();
        queue.Has(1);
        queue.First.Is(1);
        queue.RemoveFirst();
        queue.IsEmpty();
        Wrap.It(() => queue.RemoveFirst()).Throws<InvalidOperationException>();
    }

    /// <summary>
    /// Verifies that removing elements from the last position works correctly in reverse mode.
    /// </summary>
    [Fact]
    public void Reverse_RemoveLast()
    {
        // arrange
        var queue = new DoubleEdgeQueue<int>(false);
        queue.AddFirst(1);
        queue.AddFirst(2);

        // act & assert
        queue.RemoveLast();
        queue.Has(1);
        queue.First.Is(2);
        queue.RemoveLast();
        queue.IsEmpty();
        Wrap.It(() => queue.RemoveLast()).Throws<InvalidOperationException>();
    }

    /// <summary>
    /// Verifies that constructing from a non-empty IEnumerable with isDirect=true preserves insertion order
    /// and exposes the correct Count, First, and Last values.
    /// </summary>
    [Fact]
    public void EntriesCtor_NonEmptyDirect_PreservesOrderAndCount()
    {
        // arrange & act
        var queue = new DoubleEdgeQueue<int>(new[] { 10, 20, 30 }, isDirect: true);

        // assert
        queue.Has(3);
        queue.First.Is(10);
        queue.Last.Is(30);
        queue.ToArray().IsEqual(new[] { 10, 20, 30 });
    }

    /// <summary>
    /// Verifies that constructing from a non-empty IEnumerable with isDirect=false preserves the same
    /// entry order in the underlying list (directionality only affects Add/Remove, not the stored order).
    /// </summary>
    [Fact]
    public void EntriesCtor_NonEmptyReverse_SameStoredOrder()
    {
        // arrange & act — reverse mode: AddFirst/AddLast semantics are inverted but the ctor just
        // copies the IEnumerable directly into a LinkedList, so First/Last reflect insertion order.
        var queue = new DoubleEdgeQueue<int>(new[] { 10, 20, 30 }, isDirect: false);

        // assert
        queue.Has(3);
        queue.First.Is(10);
        queue.Last.Is(30);
        queue.ToArray().IsEqual(new[] { 10, 20, 30 });
    }

    /// <summary>
    /// Verifies that constructing from an empty IEnumerable yields a queue with Count == 0.
    /// </summary>
    [Fact]
    public void EntriesCtor_EmptyEnumerable_CountIsZero()
    {
        // arrange & act
        var queue = new DoubleEdgeQueue<int>(Array.Empty<int>(), isDirect: true);

        // assert
        queue.Has(0);
        queue.IsEmpty();
    }

    /// <summary>
    /// Verifies that accessing First on an empty queue throws InvalidOperationException.
    /// </summary>
    [Fact]
    public void First_EmptyQueue_ThrowsInvalidOperationException()
    {
        // arrange
        var queue = new DoubleEdgeQueue<int>(true);

        // act & assert
        Wrap.It(() => _ = queue.First).Throws<InvalidOperationException>();
    }

    /// <summary>
    /// Verifies that accessing Last on an empty queue throws InvalidOperationException.
    /// </summary>
    [Fact]
    public void Last_EmptyQueue_ThrowsInvalidOperationException()
    {
        // arrange
        var queue = new DoubleEdgeQueue<int>(true);

        // act & assert
        Wrap.It(() => _ = queue.Last).Throws<InvalidOperationException>();
    }
}
