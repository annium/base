using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Annium.Reflection;
using Annium.Testing;
using Xunit;

namespace Annium.Tests.Reflection.Type;

public class ResolveGenericArgumentsByImplementationExtensionClassTests
{
    [Fact]
    public void Param_TypeNotGeneric_ReturnEmptyTypes()
    {
        // assert
        typeof(Array).ResolveGenericArgumentsByImplementation(typeof(IEnumerable<>).GetGenericArguments()[0])
            .Is(System.Type.EmptyTypes);
    }

    [Fact]
    public void Param_TypeGenericDefined_ReturnTypeArguments()
    {
        // assert
        typeof(List<int>).ResolveGenericArgumentsByImplementation(typeof(IEnumerable<>).GetGenericArguments()[0])
            .IsEqual(new[] { typeof(int) });
    }

    [Fact]
    public void Param_StructTypeConstraintFailure_ReturnsNull()
    {
        // assert
        typeof(List<>)
            .ResolveGenericArgumentsByImplementation(typeof(IStructConstraint<>).GetGenericArguments()[0])
            .IsDefault();
    }

    [Fact]
    public void Param_DefaultConstructorConstraintFailure_ReturnsNull()
    {
        // assert
        typeof(ClassParametrized<>)
            .ResolveGenericArgumentsByImplementation(typeof(INewConstraint<>).GetGenericArguments()[0])
            .IsDefault();
    }

    [Fact]
    public void Param_ParameterConstraintFailure_ReturnsNull()
    {
        // assert
        typeof(CustomDictionary<,>)
            .ResolveGenericArgumentsByImplementation(typeof(IClassBaseConstraint<>).GetGenericArguments()[0])
            .IsDefault();
    }

    [Fact]
    public void Param_ParameterConstraintSuccess_ReturnsType()
    {
        // assert
        typeof(ClassSimple)
            .ResolveGenericArgumentsByImplementation(typeof(IClassBaseConstraint<>).GetGenericArguments()[0])
            .IsEqual(System.Type.EmptyTypes);
    }

    [Fact]
    public void Class_TypeNotGeneric_ReturnEmptyTypes()
    {
        // assert
        typeof(MemoryStream).ResolveGenericArgumentsByImplementation(typeof(Stream))
            .Is(System.Type.EmptyTypes);
    }

    [Fact]
    public void Class_TypeGenericDefined_ReturnTypeArguments()
    {
        // assert
        typeof(CustomDictionary<int, string>).ResolveGenericArgumentsByImplementation(typeof(Dictionary<,>))
            .IsEqual(new[] { typeof(int), typeof(string) });
    }

    [Fact]
    public void Class_TargetNotGeneric_ReturnsTypeArguments()
    {
        // assert
        typeof(ClassParametrized<>).ResolveGenericArgumentsByImplementation(typeof(ClassBase))
            .IsEqual(new[] { typeof(ClassParametrized<>).GetGenericArguments()[0] });
    }

    [Fact]
    public void Class_SameGenericDefinition_BuildArgs()
    {
        // assert
        typeof(List<>).ResolveGenericArgumentsByImplementation(typeof(List<int>))
            .IsEqual(new[] { typeof(int) });
    }

    [Fact]
    public void Class_NullBaseType_ReturnsNull()
    {
        // assert
        typeof(HashSet<>).ResolveGenericArgumentsByImplementation(typeof(List<int>))
            .IsDefault();
    }

    [Fact]
    public void Class_NotGenericBaseType_ReturnsNull()
    {
        // assert
        typeof(ClassParametrized<>).ResolveGenericArgumentsByImplementation(typeof(List<int>))
            .IsDefault();
    }

    [Fact]
    public void Class_BaseTypeSameGenericDefinition_BuildArgs()
    {
        // assert
        typeof(CustomDictionary<,>).ResolveGenericArgumentsByImplementation(typeof(Dictionary<int, bool>))
            .IsEqual(new[] { typeof(int), typeof(bool) });
    }

    [Fact]
    public void Class_DifferentGenericDefinition_ResolvesBase()
    {
        // assert
        typeof(ParentDictionary<,>).ResolveGenericArgumentsByImplementation(typeof(Dictionary<int, bool>))
            .IsEqual(new[] { typeof(bool), typeof(int) });
    }

    [Fact]
    public void Interface_TypeNotGeneric_ReturnEmptyTypes()
    {
        // assert
        typeof(Array).ResolveGenericArgumentsByImplementation(typeof(IEnumerable<>).GetGenericArguments()[0])
            .Is(System.Type.EmptyTypes);
    }

    [Fact]
    public void Interface_TypeGenericDefined_ReturnTypeArguments()
    {
        // assert
        typeof(List<int>).ResolveGenericArgumentsByImplementation(typeof(IEnumerable<>).GetGenericArguments()[0])
            .IsEqual(new[] { typeof(int) });
    }

    [Fact]
    public void Interface_TargetNotGeneric_ReturnsTypeArguments()
    {
        // assert
        typeof(List<>).ResolveGenericArgumentsByImplementation(typeof(IEnumerable)).IsEqual(new[]
            { typeof(List<>).GetGenericArguments()[0] });
    }

    [Fact]
    public void Interface_WithImplementation_BuildsArgs()
    {
        // assert
        typeof(Dictionary<,>).ResolveGenericArgumentsByImplementation(typeof(IReadOnlyDictionary<int, bool>))
            .IsEqual(new[] { typeof(int), typeof(bool) });
    }

    [Fact]
    public void Interface_NoImplementation_NoBaseType_ReturnsNull()
    {
        // assert
        typeof(List<>).ResolveGenericArgumentsByImplementation(typeof(IEquatable<int>))
            .IsDefault();
    }

    [Fact]
    public void Interface_NoImplementation_WithBaseType_ResolvesBase()
    {
        // assert
        typeof(ParentDictionary<,>).ResolveGenericArgumentsByImplementation(typeof(IReadOnlyDictionary<int, bool>))
            .IsEqual(new[] { typeof(bool), typeof(int) });
    }

    private class ParentDictionary<T1, T2> : CustomDictionary<T2, T1> where T2 : notnull
    {
    }

    private class CustomDictionary<T1, T2> : Dictionary<T1, T2> where T1 : notnull
    {
    }

    private class ClassParametrized<T> : ClassBase
    {
        public T X { get; }

        public ClassParametrized(T x)
        {
            X = x;
        }
    }

    private class ClassSimple : ClassBase
    {
    }

    private interface IStructConstraint<T> where T : struct
    {
    }

    private interface INewConstraint<T> where T : new()
    {
    }

    private interface IClassBaseConstraint<T> where T : ClassBase
    {
    }

    private class ClassBase
    {
    }
}