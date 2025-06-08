using Annium.Collections.Generic;
using Annium.Linq;
using Annium.Testing;
using Xunit;

namespace Annium.Tests.Collections.Generic;

/// <summary>
/// Contains unit tests for <see cref="ListSpan{T}"/> to verify span behavior.
/// </summary>
public class ListSpanTest
{
    /// <summary>
    /// Verifies that adding elements, counting, indexing, and moving the span work correctly.
    /// </summary>
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
