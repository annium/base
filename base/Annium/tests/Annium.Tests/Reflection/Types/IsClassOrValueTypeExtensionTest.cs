using System;
using System.IO;
using Annium.Reflection;
using Annium.Testing;
using Xunit;

namespace Annium.Tests.Reflection.Types;

/// <summary>
/// Contains unit tests for the IsClassOrValueType extension method.
/// </summary>
public class IsClassOrValueTypeExtensionTest
{
    /// <summary>
    /// Verifies that IsClassOrValueType throws when called on null.
    /// </summary>
    [Fact]
    public void OfNull_Throws()
    {
        // assert
        Wrap.It(() => (null as Type)!.IsConstructable()).Throws<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that IsClassOrValueType works for classes.
    /// </summary>
    [Fact]
    public void Class_Works()
    {
        // assert
        typeof(object).IsConstructable().IsTrue();
        typeof(FileInfo).IsConstructable().IsTrue();
        typeof(Stream).IsConstructable().IsFalse();
    }

    /// <summary>
    /// Verifies that IsClassOrValueType works for structs.
    /// </summary>
    [Fact]
    public void Struct_Works()
    {
        // assert
        typeof(long).IsConstructable().IsTrue();
        typeof(ValueTuple<>).IsConstructable().IsTrue();
    }
}
