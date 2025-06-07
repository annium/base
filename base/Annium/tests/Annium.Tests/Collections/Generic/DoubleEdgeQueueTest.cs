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
}
