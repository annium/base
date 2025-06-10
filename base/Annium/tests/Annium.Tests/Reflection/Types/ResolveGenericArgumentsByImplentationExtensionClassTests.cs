using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Annium.Reflection.Types;
using Annium.Testing;
using Xunit;

namespace Annium.Tests.Reflection.Types;

/// <summary>
/// Contains unit tests for the ResolveGenericArgumentsByImplementation extension method for classes.
/// </summary>
public class ResolveGenericArgumentsByImplementationExtensionClassTests
{
    /// <summary>
    /// Verifies that ResolveGenericArgumentsByImplementation returns empty types when type is not generic.
    /// </summary>
    [Fact]
    public void Param_TypeNotGeneric_ReturnEmptyTypes()
    {
        // assert
        typeof(Array)
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
        typeof(List<int>)
            .ResolveGenericArgumentsByImplementation(typeof(IEnumerable<>).GetGenericArguments()[0])
            .IsEqual(new[] { typeof(int) });
    }

    /// <summary>
    /// Verifies that ResolveGenericArgumentsByImplementation returns null when struct type constraint fails.
    /// </summary>
    [Fact]
    public void Param_StructTypeConstraintFailure_ReturnsNull()
    {
        // assert
        typeof(List<>)
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
        typeof(ClassParametrized<>)
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
        typeof(CustomDictionary<,>)
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
        typeof(ClassSimple)
            .ResolveGenericArgumentsByImplementation(typeof(IClassBaseConstraint<>).GetGenericArguments()[0])
            .IsEqual(Type.EmptyTypes);
    }

    /// <summary>
    /// Verifies that ResolveGenericArgumentsByImplementation returns empty types when type is not generic.
    /// </summary>
    [Fact]
    public void Class_TypeNotGeneric_ReturnEmptyTypes()
    {
        // assert
        typeof(MemoryStream).ResolveGenericArgumentsByImplementation(typeof(Stream)).Is(Type.EmptyTypes);
    }

    /// <summary>
    /// Verifies that ResolveGenericArgumentsByImplementation returns type arguments when type is generic and defined.
    /// </summary>
    [Fact]
    public void Class_TypeGenericDefined_ReturnTypeArguments()
    {
        // assert
        typeof(CustomDictionary<int, string>)
            .ResolveGenericArgumentsByImplementation(typeof(Dictionary<,>))
            .IsEqual(new[] { typeof(int), typeof(string) });
    }

    /// <summary>
    /// Verifies that ResolveGenericArgumentsByImplementation returns type arguments when target is not generic.
    /// </summary>
    [Fact]
    public void Class_TargetNotGeneric_ReturnsTypeArguments()
    {
        // assert
        typeof(ClassParametrized<>)
            .ResolveGenericArgumentsByImplementation(typeof(ClassBase))
            .IsEqual(new[] { typeof(ClassParametrized<>).GetGenericArguments()[0] });
    }

    /// <summary>
    /// Verifies that ResolveGenericArgumentsByImplementation builds arguments when same generic definition.
    /// </summary>
    [Fact]
    public void Class_SameGenericDefinition_BuildArgs()
    {
        // assert
        typeof(List<>).ResolveGenericArgumentsByImplementation(typeof(List<int>)).IsEqual(new[] { typeof(int) });
    }

    /// <summary>
    /// Verifies that ResolveGenericArgumentsByImplementation returns null when base type is null.
    /// </summary>
    [Fact]
    public void Class_NullBaseType_ReturnsNull()
    {
        // assert
        typeof(HashSet<>).ResolveGenericArgumentsByImplementation(typeof(List<int>)).IsDefault();
    }

    /// <summary>
    /// Verifies that ResolveGenericArgumentsByImplementation returns null when base type is not generic.
    /// </summary>
    [Fact]
    public void Class_NotGenericBaseType_ReturnsNull()
    {
        // assert
        typeof(ClassParametrized<>).ResolveGenericArgumentsByImplementation(typeof(List<int>)).IsDefault();
    }

    /// <summary>
    /// Verifies that ResolveGenericArgumentsByImplementation builds arguments when base type has same generic definition.
    /// </summary>
    [Fact]
    public void Class_BaseTypeSameGenericDefinition_BuildArgs()
    {
        // assert
        typeof(CustomDictionary<,>)
            .ResolveGenericArgumentsByImplementation(typeof(Dictionary<int, bool>))
            .IsEqual(new[] { typeof(int), typeof(bool) });
    }

    /// <summary>
    /// Verifies that ResolveGenericArgumentsByImplementation resolves base when different generic definition.
    /// </summary>
    [Fact]
    public void Class_DifferentGenericDefinition_ResolvesBase()
    {
        // assert
        typeof(ParentDictionary<,>)
            .ResolveGenericArgumentsByImplementation(typeof(Dictionary<int, bool>))
            .IsEqual(new[] { typeof(bool), typeof(int) });
    }

    /// <summary>
    /// Verifies that ResolveGenericArgumentsByImplementation returns empty types when type is not generic.
    /// </summary>
    [Fact]
    public void Interface_TypeNotGeneric_ReturnEmptyTypes()
    {
        // assert
        typeof(Array)
            .ResolveGenericArgumentsByImplementation(typeof(IEnumerable<>).GetGenericArguments()[0])
            .Is(Type.EmptyTypes);
    }

    /// <summary>
    /// Verifies that ResolveGenericArgumentsByImplementation returns type arguments when type is generic and defined.
    /// </summary>
    [Fact]
    public void Interface_TypeGenericDefined_ReturnTypeArguments()
    {
        // assert
        typeof(List<int>)
            .ResolveGenericArgumentsByImplementation(typeof(IEnumerable<>).GetGenericArguments()[0])
            .IsEqual(new[] { typeof(int) });
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
    /// Verifies that ResolveGenericArgumentsByImplementation builds arguments when with implementation.
    /// </summary>
    [Fact]
    public void Interface_WithImplementation_BuildsArgs()
    {
        // assert
        typeof(Dictionary<,>)
            .ResolveGenericArgumentsByImplementation(typeof(IReadOnlyDictionary<int, bool>))
            .IsEqual(new[] { typeof(int), typeof(bool) });
    }

    /// <summary>
    /// Verifies that ResolveGenericArgumentsByImplementation returns null when no implementation and no base type.
    /// </summary>
    [Fact]
    public void Interface_NoImplementation_NoBaseType_ReturnsNull()
    {
        // assert
        typeof(List<>).ResolveGenericArgumentsByImplementation(typeof(IEquatable<int>)).IsDefault();
    }

    /// <summary>
    /// Verifies that ResolveGenericArgumentsByImplementation resolves base when no implementation and with base type.
    /// </summary>
    [Fact]
    public void Interface_NoImplementation_WithBaseType_ResolvesBase()
    {
        // assert
        typeof(ParentDictionary<,>)
            .ResolveGenericArgumentsByImplementation(typeof(IReadOnlyDictionary<int, bool>))
            .IsEqual(new[] { typeof(bool), typeof(int) });
    }

    /// <summary>
    /// Represents a parent dictionary class that reverses the key-value types of its base class.
    /// </summary>
    /// <typeparam name="T1">The type of the first generic parameter.</typeparam>
    /// <typeparam name="T2">The type of the second generic parameter.</typeparam>
    private class ParentDictionary<T1, T2> : CustomDictionary<T2, T1>
        where T2 : notnull;

    /// <summary>
    /// Represents a custom dictionary class that extends Dictionary with additional functionality.
    /// </summary>
    /// <typeparam name="T1">The type of the first generic parameter.</typeparam>
    /// <typeparam name="T2">The type of the second generic parameter.</typeparam>
    private class CustomDictionary<T1, T2> : Dictionary<T1, T2>
        where T1 : notnull;

    /// <summary>
    /// Represents a parametrized class that extends ClassBase.
    /// </summary>
    /// <typeparam name="T">The type of the generic parameter.</typeparam>
    private class ClassParametrized<T> : ClassBase
    {
        /// <summary>
        /// Gets the value of type T.
        /// </summary>
        public T X { get; }

        /// <summary>
        /// Initializes a new instance of the ClassParametrized class.
        /// </summary>
        /// <param name="x">The value to initialize X with.</param>
        public ClassParametrized(T x)
        {
            X = x;
        }
    }

    /// <summary>
    /// Represents a simple class that extends ClassBase.
    /// </summary>
    private class ClassSimple : ClassBase;

    /// <summary>
    /// Represents an interface with a struct constraint.
    /// </summary>
    /// <typeparam name="T">The type parameter that must be a struct.</typeparam>
    private interface IStructConstraint<T>
        where T : struct;

    /// <summary>
    /// Represents an interface with a new constraint.
    /// </summary>
    /// <typeparam name="T">The type parameter that must have a parameterless constructor.</typeparam>
    private interface INewConstraint<T>
        where T : new();

    /// <summary>
    /// Represents an interface with a class base constraint.
    /// </summary>
    /// <typeparam name="T">The type parameter that must be a class and inherit from ClassBase.</typeparam>
    private interface IClassBaseConstraint<T>
        where T : ClassBase;

    /// <summary>
    /// Represents a base class for testing.
    /// </summary>
    private class ClassBase;
}
