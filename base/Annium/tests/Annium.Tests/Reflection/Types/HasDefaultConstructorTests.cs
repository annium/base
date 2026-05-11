using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Annium.Reflection;
using Annium.Testing;
using Xunit;

namespace Annium.Tests.Reflection.Types;

/// <summary>
/// Contains unit tests for the HasDefaultConstructor extension method.
/// </summary>
public class HasDefaultConstructorTests
{
    /// <summary>
    /// Verifies that HasDefaultConstructor throws when called on null.
    /// </summary>
    [Fact]
    public void HasDefaultConstructor_OfNull_Throws()
    {
        // assert
        Wrap.It(() => (null as Type)!.HasDefaultConstructor()).Throws<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that HasDefaultConstructor works for classes.
    /// </summary>
    [Fact]
    public void HasDefaultConstructor_Class_Works()
    {
        // assert
        typeof(object).HasDefaultConstructor().IsTrue();
        typeof(FileInfo).HasDefaultConstructor().IsFalse();
    }

    /// <summary>
    /// Verifies that HasDefaultConstructor works for structs. Primitives and structs without an explicit
    /// constructor are reported as having a default constructor (the implicit zero-init one); structs with
    /// any explicit constructor — and no explicit empty one — are reported as not having a default
    /// constructor (mirrors what reflection's <c>GetConstructor(Type.EmptyTypes)</c> returns).
    /// </summary>
    [Fact]
    public void HasDefaultConstructor_Struct_Works()
    {
        // assert
        typeof(long).HasDefaultConstructor().IsTrue();
        typeof(ValueTuple<int>).HasDefaultConstructor().IsFalse();
    }

    /// <summary>
    /// Verifies that HasDefaultConstructor throws for open generic type definitions, which cannot be
    /// instantiated via <see cref="Activator.CreateInstance(Type)"/>.
    /// </summary>
    [Fact]
    public void HasDefaultConstructor_OpenGeneric_Throws()
    {
        // assert
        Wrap.It(() => typeof(List<>).HasDefaultConstructor()).Throws<ArgumentException>();
        Wrap.It(() => typeof(Dictionary<,>).HasDefaultConstructor()).Throws<ArgumentException>();
        Wrap.It(() => typeof(ValueTuple<>).HasDefaultConstructor()).Throws<ArgumentException>();
    }

    /// <summary>
    /// Verifies that HasDefaultConstructor throws for other types.
    /// </summary>
    [Fact]
    public void HasDefaultConstructor_Other_Throws()
    {
        // assert
        Wrap.It(() => typeof(IEnumerable).HasDefaultConstructor()).Throws<ArgumentException>();
    }
}
