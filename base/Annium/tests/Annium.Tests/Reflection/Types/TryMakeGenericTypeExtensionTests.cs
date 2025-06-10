using System;
using Annium.Reflection.Types;
using Annium.Testing;
using Xunit;

namespace Annium.Tests.Reflection.Types;

/// <summary>
/// Contains unit tests for the TryMakeGenericType extension method.
/// </summary>
public class TryMakeGenericTypeExtensionTests
{
    /// <summary>
    /// Verifies that TryMakeGenericType throws when called on null.
    /// </summary>
    [Fact]
    public void TryMakeGenericType_OfNull_Throws()
    {
        // assert
        Wrap.It(() => (null as Type)!.TryMakeGenericType(out _)).Throws<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that TryMakeGenericType works correctly.
    /// </summary>
    [Fact]
    public void ITryMakeGenericType_Works()
    {
        // assert
        typeof(Demo<>).TryMakeGenericType(out var result).IsFalse();
        result.IsDefault();

        typeof(Demo<>).TryMakeGenericType(out result, typeof(bool)).IsFalse();
        result.IsDefault();

        typeof(Demo<>).TryMakeGenericType(out result, typeof(object)).IsTrue();
        result.Is(typeof(Demo<object>));
    }

    /// <summary>
    /// A demo class used for testing generic type creation.
    /// </summary>
    private class Demo<T>
        where T : class;
}
