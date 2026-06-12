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
        var d = new Sample2();

        // assert
        string.IsNullOrWhiteSpace(a.GetId()).IsFalse();
        a.GetId().Is(a.GetId()); // stable per instance
        a.GetId().IsNot(b.GetId()); // unique per instance
        a.GetId().Is("1"); // Sample counter starts at 1 (file-local type)
        b.GetId().Is("2");
        d.GetId().Is("1"); // Sample2 has an independent counter — not shared among types
    }
}

/// <summary>
/// Sample record for testing GetId extension.
/// </summary>
file record Sample;

/// <summary>
/// Second sample record for verifying per-type counter isolation in GetId.
/// </summary>
file record Sample2;
