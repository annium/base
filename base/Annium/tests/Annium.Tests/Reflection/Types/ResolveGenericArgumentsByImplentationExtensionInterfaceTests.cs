using System;
using System.Collections;
using System.Collections.Generic;
using Annium.Reflection;
using Annium.Testing;
using Xunit;

namespace Annium.Tests.Reflection.Types;

/// <summary>
/// Contains unit tests for the ResolveGenericArgumentsByImplementation extension method for interfaces.
/// </summary>
public class ResolveGenericArgumentsByImplementationExtensionInterfaceTests
{
    /// <summary>
    /// Verifies that ResolveGenericArgumentsByImplementation returns empty types when type is not generic.
    /// </summary>
    [Fact]
    public void Param_ReferenceTypeConstraintFailure_ReturnsNull()
    {
        // assert
        typeof(IEnumerable<>)
            .ResolveGenericArgumentsByImplementation(typeof(IClassConstraint<>).GetGenericArguments()[0])
            .IsDefault();
    }

    /// <summary>
    /// Verifies that ResolveGenericArgumentsByImplementation returns null when struct type constraint fails.
    /// </summary>
    [Fact]
    public void Param_StructTypeConstraintFailure_ReturnsNull()
    {
        // assert
        typeof(IEnumerable<>)
            .ResolveGenericArgumentsByImplementation(typeof(IStructConstraint<>).GetGenericArguments()[0])
            .IsDefault();
    }

    /// <summary>
    /// Verifies that ResolveGenericArgumentsByImplementation returns null when default constructor constraint fails.
    /// </summary>
    [Fact]
    public void Param_DefaultConstructorConstraintFailure_ReturnsNull()
    {
        // assert
        typeof(IEnumerable<>)
            .ResolveGenericArgumentsByImplementation(typeof(INewConstraint<>).GetGenericArguments()[0])
            .IsDefault();
    }

    /// <summary>
    /// Verifies that ResolveGenericArgumentsByImplementation returns null when parameter constraint fails.
    /// </summary>
    [Fact]
    public void Param_ParameterConstraintFailure_ReturnsNull()
    {
        // assert
        typeof(IEnumerable<>)
            .ResolveGenericArgumentsByImplementation(typeof(IClassBaseConstraint<>).GetGenericArguments()[0])
            .IsDefault();
    }

    /// <summary>
    /// Verifies that ResolveGenericArgumentsByImplementation returns type when parameter constraint succeeds.
    /// </summary>
    [Fact]
    public void Param_ParameterConstraintSuccess_ReturnsType()
    {
        // assert
        typeof(IEnumerable<>)
            .ResolveGenericArgumentsByImplementation(typeof(IEnumerableConstraint<>).GetGenericArguments()[0])
            .IsEqual(new[] { typeof(IEnumerable<>).GetGenericArguments()[0] });
    }

    /// <summary>
    /// Verifies that ResolveGenericArgumentsByImplementation returns empty types when type is not generic.
    /// </summary>
    [Fact]
    public void Interface_SameGenericDefinition_BuildsArgs()
    {
        // assert
        typeof(IEquatable<>)
            .ResolveGenericArgumentsByImplementation(typeof(IEquatable<bool>))
            .IsEqual(new[] { typeof(bool) });
    }

    /// <summary>
    /// Verifies that ResolveGenericArgumentsByImplementation returns type arguments when target is not generic.
    /// </summary>
    [Fact]
    public void Interface_NoImplementation_ReturnsNull()
    {
        // assert
        typeof(IEnumerable<>).ResolveGenericArgumentsByImplementation(typeof(IEquatable<bool>)).IsDefault();
    }

    /// <summary>
    /// Verifies that ResolveGenericArgumentsByImplementation resolves base when no implementation and with base type.
    /// </summary>
    [Fact]
    public void Interface_WithImplementation_BuildArgs()
    {
        // assert
        typeof(IParentOther<,>)
            .ResolveGenericArgumentsByImplementation(typeof(IBase<string[], int, bool, IEnumerable<string[]>>))
            .IsEqual(new[] { typeof(string), typeof(int) });
    }

    /// <summary>
    /// Represents a parent interface with two generic parameters, inheriting from IBase.
    /// </summary>
    /// <typeparam name="T1">The type of the first generic parameter.</typeparam>
    /// <typeparam name="T2">The type of the second generic parameter.</typeparam>
    private interface IParentOther<T1, T2> : IBase<T1[], T2, bool, IEnumerable<T1[]>>
        where T2 : struct;

    /// <summary>
    /// Represents a base interface for testing with four generic parameters.
    /// </summary>
    /// <typeparam name="T1">The first type parameter, must be a class.</typeparam>
    /// <typeparam name="T2">The second type parameter, must be a struct.</typeparam>
    /// <typeparam name="T3">The third type parameter.</typeparam>
    /// <typeparam name="T4">The fourth type parameter, must be IEnumerable of T1.</typeparam>
    private interface IBase<T1, T2, T3, T4>
        where T1 : class
        where T2 : struct
        where T4 : IEnumerable<T1>;

    /// <summary>
    /// Represents an interface with a class constraint.
    /// </summary>
    /// <typeparam name="T">The type parameter that must be a class.</typeparam>
    private interface IClassConstraint<T>
        where T : class;

    /// <summary>
    /// Represents an interface with a struct constraint.
    /// </summary>
    /// <typeparam name="T">The type parameter that must be a struct.</typeparam>
    private interface IStructConstraint<T>
        where T : struct;

    /// <summary>
    /// Represents an interface with a new() constraint.
    /// </summary>
    /// <typeparam name="T">The type parameter that must have a parameterless constructor.</typeparam>
    private interface INewConstraint<T>
        where T : new();

    /// <summary>
    /// Represents an interface with a class base constraint.
    /// </summary>
    /// <typeparam name="T">The type parameter that must inherit from ClassBase.</typeparam>
    private interface IClassBaseConstraint<T>
        where T : ClassBase;

    /// <summary>
    /// Represents an interface with an IEnumerable constraint.
    /// </summary>
    /// <typeparam name="T">The type parameter that must implement IEnumerable.</typeparam>
    private interface IEnumerableConstraint<T>
        where T : IEnumerable;

    /// <summary>
    /// Represents an interface with an IEquatable constraint.
    /// </summary>
    /// <typeparam name="T">The type parameter that must implement IEquatable&lt;T&gt;.</typeparam>
    private interface IEquatableConstraint<T>
        where T : IEquatable<T>;

    /// <summary>
    /// Represents a base class for testing.
    /// </summary>
    private class ClassBase;
}
