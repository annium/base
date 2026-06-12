using System;
using Annium.Collections.Generic;
using Annium.Testing;
using Xunit;

namespace Annium.Tests.Internal.Collections.Generic;

/// <summary>
/// Exact-message tests for the centralised IndexOutOfRangeMessage format. The internal type is
/// reached transitively through the public <see cref="ListSpan{T}"/> API so the assertions lock the
/// wording without taking a dependency on the internal helper itself.
/// </summary>
public class IndexOutOfRangeMessageTests
{
    /// <summary>
    /// Negative index produces the documented format <c>"Index N is out of range [0;count-1]"</c>.
    /// </summary>
    [Fact]
    public void For_NegativeIndex_FormatsAsExpected()
    {
        var span = new ListSpan<int>(new[] { 10, 20, 30 }, 0, 3);

        var ex = Wrap.It(() =>
            {
                var _ = span[-1];
            })
            .Throws<ArgumentOutOfRangeException>();

        ex.Message.Contains("Index -1 is out of range [0;2]").IsTrue();
    }

    /// <summary>
    /// Index equal to count produces the same format with the offending value substituted.
    /// </summary>
    [Fact]
    public void For_IndexEqualsCount_FormatsAsExpected()
    {
        var span = new ListSpan<int>(new[] { 10, 20, 30 }, 0, 3);

        var ex = Wrap.It(() =>
            {
                var _ = span[3];
            })
            .Throws<ArgumentOutOfRangeException>();

        ex.Message.Contains("Index 3 is out of range [0;2]").IsTrue();
    }
}
