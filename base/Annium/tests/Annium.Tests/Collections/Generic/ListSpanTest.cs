using Annium.Linq;
using Annium.Testing;
using Xunit;

namespace Annium.Tests.Collections.Generic;

public class ListSpanTest
{
    [Fact]
    public void Add_Count_Index_Move()
    {
        // arrange
        var data = new[] { 1, 2, 3, 4, 5 };

        // act & assert - move forward
        var span = data.ToListSpan(0, 2);
        span.Count.Is(2);
        span.Move(-1).IsFalse();
        span[0].Is(1);
        span[1].Is(2);
        span.Move(3).IsTrue();
        span[0].Is(4);
        span[1].Is(5);

        // act & assert - move backward
        span = data.ToListSpan(3, 2);
        span.Count.Is(2);
        span.Move(1).IsFalse();
        span[0].Is(4);
        span[1].Is(5);
        span.Move(-3).IsTrue();
        span[0].Is(1);
        span[1].Is(2);
    }
}