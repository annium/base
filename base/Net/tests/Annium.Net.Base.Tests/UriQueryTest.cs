using System.Collections.Generic;
using Annium.Testing;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace Annium.Net.Base.Tests;

/// <summary>
/// Test class for UriQuery functionality.
/// </summary>
public class UriQueryTest
{
    /// <summary>
    /// Tests that parsing query strings works correctly.
    /// </summary>
    [Fact]
    public void Parse_Works()
    {
        // act
        var qn = UriQuery.Parse(null!);
        var q1 = UriQuery.Parse("");
        var q2 = UriQuery.Parse("value");
        var q3 = UriQuery.Parse("a=1&b=2&b=3");
        var q4 = UriQuery.Parse("a=1&b=2,3");

        // assert
        qn.ToString().Is("");
        q1.ToString().Is("");
        q2.ToString().Is("?value=");
        q3.ToString().Is("?a=1&b=2&b=3");
        q4.ToString().Is("?a=1&b=2%2C3");
    }

    /// <summary>
    /// Regression test for <c>UriQuery.CopyTo</c> which previously called <c>_data.Clear()</c>,
    /// wiping the query instead of copying its contents to the supplied array. Any LINQ operator
    /// or serializer walking the <see cref="System.Collections.Generic.ICollection{T}"/> contract
    /// silently destroyed the query.
    /// </summary>
    [Fact]
    public void CopyTo_FillsArrayAndPreservesOriginal()
    {
        // arrange
        var query = UriQuery.Parse("a=1&b=2");
        var originalCount = ((ICollection<KeyValuePair<string, StringValues>>)query).Count;
        var array = new KeyValuePair<string, StringValues>[originalCount];

        // act
        query.CopyTo(array, 0);

        // assert — original is intact
        ((ICollection<KeyValuePair<string, StringValues>>)query).Count.Is(originalCount);
        query["a"].ToString().Is("1");
        query["b"].ToString().Is("2");

        // assert — destination array is populated
        array.Length.Is(originalCount);
        var byKey = new Dictionary<string, string>();
        foreach (var kv in array)
            byKey[kv.Key] = kv.Value.ToString();
        byKey["a"].Is("1");
        byKey["b"].Is("2");
    }

    /// <summary>
    /// Tests that UriQuery equality comparison works correctly.
    /// </summary>
    [Fact]
    public void Equality_Works()
    {
        // act
        var hashSet = new HashSet<UriQuery>();
        var q1 = UriQuery.Parse("a=1&b=3");
        var q2 = UriQuery.Parse("?b=2&a=1&b=3");
        var q3 = UriQuery.Parse("a=1&b=2&b=3");

        // assert
        (q1 == q2).IsFalse();
        (q1 == q3).IsFalse();
        (q2 == q3).IsTrue();
        hashSet.Add(q1).IsTrue();
        hashSet.Add(q2).IsTrue();
        hashSet.Add(q3).IsFalse();
    }
}
