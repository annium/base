using System;
using System.Collections.Generic;
using System.IO;
using Annium.Reflection;
using Annium.Testing;
using Xunit;

namespace Annium.Tests.Reflection.Types;

/// <summary>
/// Contains unit tests for the ResolveByImplementation extension method.
/// </summary>
public class ResolveByImplementationExtensionTests
{
    /// <summary>
    /// Verifies that passing null as the type parameter throws ArgumentNullException.
    /// </summary>
    [Fact]
    public void TypeNull_Throws()
    {
        // assert
        Wrap.It(() => (null as Type)!.ResolveByImplementation(typeof(byte))).Throws<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that passing null as the target parameter throws ArgumentNullException.
    /// </summary>
    [Fact]
    public void TargetNull_Throws()
    {
        // assert
        Wrap.It(() => typeof(byte).ResolveByImplementation((null as Type)!)).Throws<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that when a type is defined and assignable to the target type, it returns the correct type.
    /// </summary>
    [Fact]
    public void Defined_Assignable_ReturnsType()
    {
        // assert
        typeof(MemoryStream).ResolveByImplementation(typeof(Stream)).Is(typeof(MemoryStream));
    }

    /// <summary>
    /// Verifies that when a type is defined but not assignable to the target type, it returns null.
    /// </summary>
    [Fact]
    public void Defined_NotAssignable_ReturnsType()
    {
        // assert
        typeof(FileInfo).ResolveByImplementation(typeof(Stream)).IsDefault();
    }

    /// <summary>
    /// Verifies that when a generic type is not resolved, it returns null.
    /// </summary>
    [Fact]
    public void Generic_NotResolved_ReturnsNull()
    {
        // assert
        typeof(List<>).ResolveByImplementation(typeof(Stream)).IsDefault();
    }

    /// <summary>
    /// Verifies that when resolving a generic parameter, it returns the correct parameter type.
    /// </summary>
    [Fact]
    public void GenericParameter_ReturnsParameter()
    {
        // assert
        typeof(List<>).GetGenericArguments()[0].ResolveByImplementation(typeof(int)).Is(typeof(int));
    }

    /// <summary>
    /// Verifies that when resolving a generic type, it returns the correct constructed type.
    /// </summary>
    [Fact]
    public void GenericType_ReturnsType()
    {
        // assert
        typeof(List<>).ResolveByImplementation(typeof(IEnumerable<int>)).Is(typeof(List<int>));
    }
}
