using System.Linq;
using Annium.Collections.Generic;
using Annium.Testing;
using Xunit;

namespace Annium.Tests.Collections.Generic;

public class FixedIndexedQueueTest
{
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