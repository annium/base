using System;
using System.Collections;
using System.Collections.Generic;
using Annium.Testing;
using Xunit;

namespace Annium.Tests;

/// <summary>
/// Contains unit tests for array-related type extension methods.
/// </summary>
public class TypeArrayExtensionsTest
{
    /// <summary>
    /// Verifies that IsEnumerable correctly identifies enumerable types.
    /// </summary>
    [Fact]
    public void IsEnumerable_Ok()
    {
        typeof(string).IsEnumerable().IsFalse();
        typeof(Array).IsEnumerable().IsTrue();
        typeof(string[]).IsEnumerable().IsTrue();
        typeof(IEnumerable).IsEnumerable().IsTrue();
        typeof(IReadOnlyDictionary<,>).IsEnumerable().IsTrue();
    }

    /// <summary>
    /// Verifies that TryGetArrayElementType correctly extracts element types from array and collection types.
    /// </summary>
    [Fact]
    public void TryGetArrayElementType_Ok()
    {
        typeof(string).TryGetArrayElementType(out _).IsFalse();
        typeof(Array).TryGetArrayElementType(out _).IsFalse();
        typeof(string[]).TryGetArrayElementType(out var elementType).IsTrue();
        elementType.Is(typeof(string));
        typeof(IEnumerable).TryGetArrayElementType(out _).IsFalse();
        typeof(IEnumerable<int>).TryGetArrayElementType(out elementType).IsTrue();
        elementType.Is(typeof(int));
        typeof(IReadOnlyDictionary<,>).TryGetArrayElementType(out elementType).IsTrue();
        elementType!.GetGenericTypeDefinition().Is(typeof(KeyValuePair<,>));
    }
}
