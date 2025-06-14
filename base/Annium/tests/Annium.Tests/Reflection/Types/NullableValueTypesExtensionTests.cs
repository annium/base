using System;
using Annium.Reflection;
using Annium.Testing;
using Xunit;

namespace Annium.Tests.Reflection.Types;

/// <summary>
/// Contains unit tests for the NullableValueTypes extension methods.
/// </summary>
public class NullableValueTypesExtensionTests
{
    /// <summary>
    /// Verifies that IsNotNullableValueType throws when called on null.
    /// </summary>
    [Fact]
    public void IsNotNullableValueType_OfNull_Throws()
    {
        // assert
        Wrap.It(() => (null as Type)!.IsNotNullableValueType()).Throws<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that IsNullableValueType throws when called on null.
    /// </summary>
    [Fact]
    public void IsNullableValueType_OfNull_Throws()
    {
        // assert
        Wrap.It(() => (null as Type)!.IsNullableValueType()).Throws<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that IsNotNullableValueType works correctly.
    /// </summary>
    [Fact]
    public void IsNotNullableValueType_Works()
    {
        // assert
        typeof(object).IsNotNullableValueType().IsFalse();
        typeof(bool).IsNotNullableValueType().IsTrue();
        typeof(bool?).IsNotNullableValueType().IsFalse();
    }

    /// <summary>
    /// Verifies that IsNullableValueType works correctly.
    /// </summary>
    [Fact]
    public void IsNullableValueType_Works()
    {
        // assert
        typeof(object).IsNullableValueType().IsFalse();
        typeof(bool).IsNullableValueType().IsFalse();
        typeof(bool?).IsNullableValueType().IsTrue();
    }
}
