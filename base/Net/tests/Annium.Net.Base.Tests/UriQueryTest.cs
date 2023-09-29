using System.Collections.Generic;
using Annium.Testing;
using Xunit;

namespace Annium.Net.Base.Tests;

public class UriQueryTest
{
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