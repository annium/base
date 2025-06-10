using System;
using System.Collections;
using System.IO;
using Annium.Reflection.Types;
using Annium.Testing;
using Xunit;

namespace Annium.Tests.Reflection.Types;

/// <summary>
/// Contains unit tests for the ResolveGenericArgumentsByImplementation extension method for parameters.
/// </summary>
public class ResolveGenericArgumentsByImplementationExtensionParameterTests
{
    /// <summary>
    /// Verifies that ResolveGenericArgumentsByImplementation returns empty types when type is not generic.
    /// </summary>
    [Fact]
    public void Param_ReferenceTypeConstraintFailure_ReturnsNull()
    {
        // assert
        typeof(IStructConstraint<>)
            .GetGenericArguments()[0]
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
        typeof(IClassConstraint<>)
            .GetGenericArguments()[0]
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
        typeof(IClassConstraint<>)
            .GetGenericArguments()[0]
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
        typeof(IClassConstraint<>)
            .GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(IEnumerableConstraint<>).GetGenericArguments()[0])
            .IsDefault();
    }

    /// <summary>
    /// Verifies that ResolveGenericArgumentsByImplementation returns type when parameter constraint succeeds.
    /// </summary>
    [Fact]
    public void Param_ParameterConstraintSuccess_ReturnsType()
    {
        // assert
        typeof(IClassBaseConstraint<>)
            .GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(IClassBaseConstraint<>).GetGenericArguments()[0])
            .IsEqual(new[] { typeof(IClassBaseConstraint<>).GetGenericArguments()[0] });
    }

    /// <summary>
    /// Verifies that ResolveGenericArgumentsByImplementation returns empty types when type is not generic.
    /// </summary>
    [Fact]
    public void Class_StructTypeConstraintFailure_ReturnsNull()
    {
        // assert
        typeof(IStructConstraint<>)
            .GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(string))
            .IsDefault();
    }

    /// <summary>
    /// Verifies that ResolveGenericArgumentsByImplementation returns type arguments when type is generic and defined.
    /// </summary>
    [Fact]
    public void Class_DefaultConstructorConstraintFailure_ReturnsNull()
    {
        // assert
        typeof(INewConstraint<>)
            .GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(string))
            .IsDefault();
    }

    /// <summary>
    /// Verifies that ResolveGenericArgumentsByImplementation returns type arguments when target is not generic.
    /// </summary>
    [Fact]
    public void Class_ParameterConstraintFailure_ReturnsNull()
    {
        // assert
        typeof(IEnumerableConstraint<>)
            .GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(FileInfo))
            .IsDefault();
    }

    /// <summary>
    /// Verifies that ResolveGenericArgumentsByImplementation builds arguments when same generic definition.
    /// </summary>
    [Fact]
    public void Class_ParameterConstraintSuccess_ReturnsType()
    {
        // assert
        typeof(RecurringBase<>)
            .GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(RecurringBase<RecurringDerived>))
            .IsEqual(new[] { typeof(RecurringBase<RecurringDerived>) });
    }

    /// <summary>
    /// Verifies that ResolveGenericArgumentsByImplementation returns null when base type is null.
    /// </summary>
    [Fact]
    public void Struct_ReferenceTypeConstraintFailure_ReturnsNull()
    {
        // assert
        typeof(IClassConstraint<>)
            .GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(long))
            .IsDefault();
    }

    /// <summary>
    /// Verifies that ResolveGenericArgumentsByImplementation returns null when base type is not generic.
    /// </summary>
    [Fact]
    public void Struct_StructTypeConstraintFailure_ReturnsNull()
    {
        // assert
        typeof(IStructConstraint<>)
            .GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(long?))
            .IsDefault();
    }

    /// <summary>
    /// Verifies that ResolveGenericArgumentsByImplementation builds arguments when base type has same generic definition.
    /// </summary>
    [Fact]
    public void Struct_DefaultConstructorConstraintFailure_ReturnsNull()
    {
        // assert
        typeof(INewConstraint<>)
            .GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(ValueTuple<>))
            .IsDefault();
    }

    /// <summary>
    /// Verifies that ResolveGenericArgumentsByImplementation resolves base when different generic definition.
    /// </summary>
    [Fact]
    public void Struct_ParameterConstraintFailure_ReturnsNull()
    {
        // assert
        typeof(IEnumerableConstraint<>)
            .GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(bool))
            .IsDefault();
    }

    /// <summary>
    /// Verifies that ResolveGenericArgumentsByImplementation returns empty types when type is not generic.
    /// </summary>
    [Fact]
    public void Interface_ReferenceTypeConstraintFailure_ReturnsNull()
    {
        // assert
        typeof(IClassConstraint<>)
            .GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(IEnumerable))
            .IsDefault();
    }

    /// <summary>
    /// Verifies that ResolveGenericArgumentsByImplementation returns null when target is not implemented.
    /// </summary>
    [Fact]
    public void Interface_StructTypeConstraintFailure_ReturnsNull()
    {
        // assert
        typeof(IStructConstraint<>)
            .GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(IEnumerable))
            .IsDefault();
    }

    /// <summary>
    /// Verifies that ResolveGenericArgumentsByImplementation returns null when target is not implemented.
    /// </summary>
    [Fact]
    public void Interface_DefaultConstructorConstraintFailure_ReturnsNull()
    {
        // assert
        typeof(INewConstraint<>)
            .GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(IEnumerable))
            .IsDefault();
    }

    /// <summary>
    /// Verifies that ResolveGenericArgumentsByImplementation returns null when no implementation and no base type.
    /// </summary>
    [Fact]
    public void Interface_ParameterConstraintFailure_ReturnsNull()
    {
        // assert
        typeof(IEquatableConstraint<>)
            .GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(IEnumerable))
            .IsDefault();
    }

    /// <summary>
    /// Verifies that ResolveGenericArgumentsByImplementation resolves base when no implementation and with base type.
    /// </summary>
    [Fact]
    public void Interface_ParameterConstraintSuccess_ReturnsType()
    {
        // assert
        typeof(IEquatableConstraint<>)
            .GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(IEquatable<string>))
            .IsEqual(new[] { typeof(IEquatable<string>) });
    }

    /// <summary>
    /// A derived class that recursively references itself through its base class.
    /// </summary>
    private class RecurringDerived : RecurringBase<RecurringDerived>;

    /// <summary>
    /// A base class that recursively references itself through its generic parameter.
    /// </summary>
    private class RecurringBase<T>
        where T : RecurringBase<T>;

    /// <summary>
    /// Interface constraint that requires the type parameter to implement IEquatable.
    /// </summary>
    private interface IEquatableConstraint<T>
        where T : IEquatable<T>;

    /// <summary>
    /// Interface constraint that requires the type parameter to be a class.
    /// </summary>
    private interface IClassConstraint<T>
        where T : class;

    /// <summary>
    /// Interface constraint that requires the type parameter to be a struct.
    /// </summary>
    private interface IStructConstraint<T>
        where T : struct;

    /// <summary>
    /// Interface constraint that requires the type parameter to have a default constructor.
    /// </summary>
    private interface INewConstraint<T>
        where T : new();

    /// <summary>
    /// Interface constraint that requires the type parameter to be a class and implement a base interface.
    /// </summary>
    private interface IClassBaseConstraint<T>
        where T : ClassBase;

    /// <summary>
    /// Interface constraint that requires the type parameter to implement IEnumerable.
    /// </summary>
    private interface IEnumerableConstraint<T>
        where T : IEnumerable;

    /// <summary>
    /// Base class for testing class constraints.
    /// </summary>
    private class ClassBase;
}
