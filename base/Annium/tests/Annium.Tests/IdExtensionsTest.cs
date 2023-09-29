using Annium.Testing;
using Xunit;

namespace Annium.Tests;

public class IdExtensionsTest
{
    [Fact]
    public void GetId_IsStablyUniquePerObject_NotSharedAmongTypes()
    {
        // arrange
        var a = new Sample();
        var b = new Sample();
        var c = new { };

        // assert
        string.IsNullOrWhiteSpace(a.GetId()).IsFalse();
        a.GetId().Is(a.GetId());
        a.GetId().Is("1");
        b.GetId().Is("2");
        c.GetId().Is("1");
    }
}

file record Sample;