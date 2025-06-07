using System.Reflection;
using Annium.Testing;
using Xunit;

namespace Annium.Tests;

/// <summary>
/// Contains unit tests for base type extension methods.
/// </summary>
public class TypeBaseExtensionsTest
{
    /// <summary>
    /// Verifies that DefaultValue returns the correct default value for different types.
    /// </summary>
    [Fact]
    public void DefaultValue_Ok()
    {
        typeof(int).DefaultValue().Is(0);
        typeof(string[]).DefaultValue().Is(null);
        typeof(string).DefaultValue().Is(null);
    }

    /// <summary>
    /// Verifies that IsScalar correctly identifies scalar types.
    /// </summary>
    [Fact]
    public void IsScalar_Ok()
    {
        typeof(int).IsScalar().IsTrue();
        typeof(string[]).IsScalar().IsFalse();
        typeof(string).IsScalar().IsTrue();
        typeof(decimal).IsScalar().IsTrue();
        typeof(TypeAttributes).IsScalar().IsTrue();
    }
}
