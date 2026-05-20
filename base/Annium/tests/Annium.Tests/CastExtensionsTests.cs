using System;
using Annium.Testing;
using Xunit;

namespace Annium.Tests;

/// <summary>
/// Tests for <see cref="CastExtensions"/>. Closes the TG4 zero-coverage gap.
/// </summary>
public class CastExtensionsTests
{
    /// <summary>Verifies CastTo returns the boxed value when the runtime type matches.</summary>
    [Fact]
    public void CastTo_CompatibleType_ReturnsValue()
    {
        object value = "hello";
        value.CastTo<string>().Is("hello");
    }

    /// <summary>Verifies CastTo throws InvalidCastException when the cast is incompatible.</summary>
    [Fact]
    public void CastTo_IncompatibleType_Throws()
    {
        object value = "hello";
        Wrap.It(() => value.CastTo<int>()).Throws<InvalidCastException>();
    }

    /// <summary>Verifies TryCastTo returns the value when the runtime type matches a reference type.</summary>
    [Fact]
    public void TryCastTo_MatchingType_ReturnsInstance()
    {
        object value = "hello";
        value.TryCastTo<string>().Is("hello");
    }

    /// <summary>Verifies TryCastTo returns null when the cast doesn't match.</summary>
    [Fact]
    public void TryCastTo_NonMatchingType_ReturnsNull()
    {
        object value = 42;
        value.TryCastTo<string>().IsDefault();
    }
}
