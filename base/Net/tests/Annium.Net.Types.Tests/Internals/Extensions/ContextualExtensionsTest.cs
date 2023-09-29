using System;
using System.Collections.Generic;
using Annium.Net.Types.Internal.Extensions;
using Annium.Testing;
using Namotion.Reflection;
using Xunit;

namespace Annium.Net.Types.Tests.Internals.Extensions;

public class ContextualExtensionsTest
{
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

    [Fact]
    public void Generic_Bound()
    {
        // arrange
        var target = typeof(Sample<string>).ToContextualType().GetProperty(nameof(Sample<int>.Value))!.AccessorType;

        // act
        var args = target.GetGenericArguments();

        // assert
        args.Has(1);
        args.At(0).Type.Is(typeof(string));
        args.At(0).Nullability.Is(Nullability.Nullable);
    }
}

file record Sample<T>(List<T?> Value);