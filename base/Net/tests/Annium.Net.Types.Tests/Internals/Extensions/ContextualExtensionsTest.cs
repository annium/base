using System;
using System.Collections.Generic;
using Annium.Net.Types.Internal.Extensions;
using Annium.Testing;
using Namotion.Reflection;
using Xunit;

namespace Annium.Net.Types.Tests.Internals.Extensions;

/// <summary>
/// Tests for contextual type extension methods
/// </summary>
public class ContextualExtensionsTest
{
    /// <summary>
    /// Tests getting generic arguments from array types
    /// </summary>
    [Fact]
    public void Array()
    {
        // arrange
        var target = typeof(int[]).ToContextualType();

        // act
        var args = target.GetGenericArguments();

        // assert
        args.IsEmpty();
    }

    /// <summary>
    /// Tests getting generic arguments from non-generic types
    /// </summary>
    [Fact]
    public void NonGeneric()
    {
        // arrange
        var target = typeof(Array).ToContextualType();

        // act
        var args = target.GetGenericArguments();

        // assert
        args.IsEmpty();
    }

    /// <summary>
    /// Tests getting generic arguments from unbound generic types
    /// </summary>
    [Fact]
    public void Generic_Unbound()
    {
        // arrange
        var target = typeof(Sample<>).ToContextualType();

        // act
        var args = target.GetGenericArguments();

        // assert
        args.Has(1);
        args.At(0).Type.IsGenericTypeParameter.IsTrue();
    }

    /// <summary>
    /// Tests getting generic arguments from bound generic types
    /// </summary>
    [Fact]
    public void Generic_Bound()
    {
        // arrange
        var target = typeof(Sample<string>).ToContextualType().GetProperty(nameof(Sample<>.Value))!.AccessorType;

        // act
        var args = target.GetGenericArguments();

        // assert
        args.Has(1);
        args.At(0).Type.Is(typeof(string));
        args.At(0).Nullability.Is(Nullability.Nullable);
    }
}

/// <summary>
/// Sample generic record for testing contextual extensions
/// </summary>
/// <typeparam name="T">The generic type parameter</typeparam>
/// <param name="Value">The list value</param>
file record Sample<T>(List<T?> Value);
