using System;
using System.Collections;
using System.Collections.Generic;
using Annium.Reflection.Types;
using Annium.Testing;
using Xunit;

namespace Annium.Tests.Reflection.Types;

/// <summary>
/// Contains unit tests for the ResolveGenericArgumentsByImplementation extension method for structs.
/// </summary>
public class ResolveGenericArgumentsByImplementationExtensionStructTests
{
    /// <summary>
    /// Verifies that ResolveGenericArgumentsByImplementation returns empty types when type is not generic.
    /// </summary>
    [Fact]
    public void Param_TypeNotGeneric_ReturnEmptyTypes()
    {
        // assert
        typeof(long)
            .ResolveGenericArgumentsByImplementation(typeof(IEnumerable<>).GetGenericArguments()[0])
            .Is(Type.EmptyTypes);
    }

    /// <summary>
    /// Verifies that ResolveGenericArgumentsByImplementation returns type arguments when type is generic and defined.
    /// </summary>
    [Fact]
    public void Param_TypeGenericDefined_ReturnTypeArguments()
    {
        // assert
        typeof(ValueTuple<int>)
            .ResolveGenericArgumentsByImplementation(typeof(IEnumerable<>).GetGenericArguments()[0])
            .IsEqual(new[] { typeof(int) });
    }

    /// <summary>
    /// Verifies that ResolveGenericArgumentsByImplementation returns null when struct type constraint fails.
    /// </summary>
    [Fact]
    public void Param_ReferenceTypeConstraintFailure_ReturnsNull()
    {
        // assert
        typeof(ValueTuple<>)
            .ResolveGenericArgumentsByImplementation(typeof(IClassConstraint<>).GetGenericArguments()[0])
            .IsDefault();
    }

    /// <summary>
    /// Verifies that ResolveGenericArgumentsByImplementation returns null when default constructor constraint fails.
    /// </summary>
    [Fact]
    public void Param_DefaultConstructorConstraintFailure_ReturnsNull()
    {
        // assert
        typeof(ValueTuple<>)
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
        typeof(ValueTuple<>)
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
        typeof(StructEnumerable)
            .ResolveGenericArgumentsByImplementation(typeof(IEnumerableConstraint<>).GetGenericArguments()[0])!
            .IsEqual(Type.EmptyTypes);
    }

    /// <summary>
    /// Verifies that ResolveGenericArgumentsByImplementation returns empty types when type is not generic.
    /// </summary>
    [Fact]
    public void Struct_SameGenericDefinition_BuildArgs()
    {
        // assert
        typeof(ValueTuple<,>)
            .ResolveGenericArgumentsByImplementation(typeof(ValueTuple<int, bool>))!
            .IsEqual(new[] { typeof(int), typeof(bool) });
    }

    /// <summary>
    /// Verifies that ResolveGenericArgumentsByImplementation returns null when base type is not generic.
    /// </summary>
    [Fact]
    public void Struct_DifferentGenericDefinition_ReturnsNull()
    {
        // assert
        typeof(ValueTuple<,>)
            .ResolveGenericArgumentsByImplementation(typeof(ValueTuple<int, string, bool>))
            .IsDefault();
    }

    /// <summary>
    /// Verifies that ResolveGenericArgumentsByImplementation returns empty types when type is not generic.
    /// </summary>
    [Fact]
    public void Interface_TypeNotGeneric_ReturnEmptyTypes()
    {
        // assert
        typeof(StructEnumerable).ResolveGenericArgumentsByImplementation(typeof(IEnumerable)).Is(Type.EmptyTypes);
    }

    /// <summary>
    /// Verifies that ResolveGenericArgumentsByImplementation returns type arguments when type is generic and defined.
    /// </summary>
    [Fact]
    public void Interface_TypeGenericDefined_ReturnTypeArguments()
    {
        // assert
        typeof(BaseStruct<string, bool, int, IEnumerable<string>>)
            .ResolveGenericArgumentsByImplementation(typeof(IBase<,,,>))
            .IsEqual(new[] { typeof(string), typeof(bool), typeof(int), typeof(IEnumerable<string>) });
    }

    /// <summary>
    /// Verifies that ResolveGenericArgumentsByImplementation returns type arguments when target is not generic.
    /// </summary>
    [Fact]
    public void Interface_TargetNotGeneric_ReturnsTypeArguments()
    {
        // assert
        typeof(List<>)
            .ResolveGenericArgumentsByImplementation(typeof(IEnumerable))
            .IsEqual(new[] { typeof(List<>).GetGenericArguments()[0] });
    }

    /// <summary>
    /// Verifies that ResolveGenericArgumentsByImplementation returns null when no implementation and no base type.
    /// </summary>
    [Fact]
    public void Interface_NoImplementation_ReturnsNull()
    {
        // assert
        typeof(ValueTuple<,>).ResolveGenericArgumentsByImplementation(typeof(IEquatable<bool>)).IsDefault();
    }

    /// <summary>
    /// Verifies that ResolveGenericArgumentsByImplementation builds arguments when interface is implemented.
    /// </summary>
    [Fact]
    public void Interface_WithImplementation_BuildArgs()
    {
        // assert
        typeof(BaseStruct<,,,>)
            .ResolveGenericArgumentsByImplementation(typeof(IBase<string, int, bool, IEnumerable<string>>))!
            .IsEqual(new[] { typeof(string), typeof(int), typeof(bool), typeof(IEnumerable<string>) });
    }

    /// <summary>
    /// A struct used for testing generic parameter constraints.
    /// </summary>
    private struct BaseStruct<T1, T2, T3, T4> : IBase<T1, T2, T3, T4>
        where T1 : class
        where T2 : struct
        where T4 : IEnumerable<T1>;

    /// <summary>
    /// An interface used for testing base constraints.
    /// </summary>
    private interface IBase<T1, T2, T3, T4>
        where T1 : class
        where T2 : struct
        where T4 : IEnumerable<T1>;

    /// <summary>
    /// A struct used for testing enumerable constraints.
    /// </summary>
    private struct StructEnumerable : IEnumerable
    {
        /// <summary>
        /// Gets the enumerator for the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// An interface used for testing class constraints.
    /// </summary>
    private interface IClassConstraint<T>
        where T : class;

    /// <summary>
    /// An interface used for testing new constraints.
    /// </summary>
    private interface INewConstraint<T>
        where T : new();

    /// <summary>
    /// An interface used for testing enumerable constraints.
    /// </summary>
    private interface IEnumerableConstraint<T>
        where T : IEnumerable;
}
