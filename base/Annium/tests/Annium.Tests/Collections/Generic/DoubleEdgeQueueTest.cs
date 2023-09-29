using System;
using System.Linq;
using Annium.Collections.Generic;
using Annium.Testing;
using Xunit;

namespace Annium.Tests.Collections.Generic;

public class DoubleEdgeQueueTest
{
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