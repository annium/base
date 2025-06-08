using System;
using System.Collections;
using System.IO;
using Annium.Reflection;
using Annium.Testing;
using Xunit;

namespace Annium.Tests.Reflection.Type;

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
        Wrap.It(() => (null as System.Type)!.HasDefaultConstructor()).Throws<ArgumentNullException>();
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
    /// Verifies that HasDefaultConstructor works for structs.
    /// </summary>
    [Fact]
    public void HasDefaultConstructor_Struct_Works()
    {
        // assert
        typeof(long).HasDefaultConstructor().IsTrue();
        typeof(ValueTuple<>).HasDefaultConstructor().IsFalse();
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
