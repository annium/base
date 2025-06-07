using Annium.Testing;
using Xunit;

namespace Annium.Tests;

/// <summary>
/// Contains unit tests for ID extension methods.
/// </summary>
public class IdExtensionsTest
{
    /// <summary>
    /// Verifies that GetId is stably unique per object and not shared among types.
    /// </summary>
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

/// <summary>
/// Sample record for testing GetId extension.
/// </summary>
file record Sample;
