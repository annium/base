using System;
using System.Collections;
using System.Collections.Generic;
using Annium.Reflection;
using Annium.Testing;
using Xunit;

namespace Annium.Tests.Reflection.Type;

public class ResolveGenericArgumentsByImplementationExtensionStructTests
{
    [Fact]
    public void Param_TypeNotGeneric_ReturnEmptyTypes()
    {
        // assert
        typeof(long).ResolveGenericArgumentsByImplementation(typeof(IEnumerable<>).GetGenericArguments()[0])
            .Is(System.Type.EmptyTypes);
    }

    [Fact]
    public void Param_TypeGenericDefined_ReturnTypeArguments()
    {
        // assert
        typeof(ValueTuple<int>).ResolveGenericArgumentsByImplementation(typeof(IEnumerable<>).GetGenericArguments()[0])
            .IsEqual(new[] { typeof(int) });
    }

    [Fact]
    public void Param_ReferenceTypeConstraintFailure_ReturnsNull()
    {
        // assert
        typeof(ValueTuple<>)
            .ResolveGenericArgumentsByImplementation(typeof(IClassConstraint<>).GetGenericArguments()[0])
            .IsDefault();
    }

    [Fact]
    public void Param_DefaultConstructorConstraintFailure_ReturnsNull()
    {
        // assert
        typeof(ValueTuple<>)
            .ResolveGenericArgumentsByImplementation(typeof(INewConstraint<>).GetGenericArguments()[0])
            .IsDefault();
    }

    [Fact]
    public void Param_ParameterConstraintFailure_ReturnsNull()
    {
        // assert
        typeof(ValueTuple<>)
            .ResolveGenericArgumentsByImplementation(typeof(IEnumerableConstraint<>).GetGenericArguments()[0])
            .IsDefault();
    }

    [Fact]
    public void Param_ParameterConstraintSuccess_ReturnsType()
    {
        // assert
        typeof(StructEnumerable)
            .ResolveGenericArgumentsByImplementation(
                typeof(IEnumerableConstraint<>).GetGenericArguments()[0])!
            .IsEqual(System.Type.EmptyTypes);
    }

    [Fact]
    public void Struct_SameGenericDefinition_BuildArgs()
    {
        // assert
        typeof(ValueTuple<,>).ResolveGenericArgumentsByImplementation(typeof(ValueTuple<int, bool>))!
            .IsEqual(new[] { typeof(int), typeof(bool) });
    }

    [Fact]
    public void Struct_DifferentGenericDefinition_ReturnsNull()
    {
        // assert
        typeof(ValueTuple<,>).ResolveGenericArgumentsByImplementation(typeof(ValueTuple<int, string, bool>))
            .IsDefault();
    }

    [Fact]
    public void Interface_TypeNotGeneric_ReturnEmptyTypes()
    {
        // assert
        typeof(StructEnumerable).ResolveGenericArgumentsByImplementation(typeof(IEnumerable))
            .Is(System.Type.EmptyTypes);
    }

    [Fact]
    public void Interface_TypeGenericDefined_ReturnTypeArguments()
    {
        // assert
        typeof(BaseStruct<string, bool, int, IEnumerable<string>>).ResolveGenericArgumentsByImplementation(typeof(IBase<,,,>))
            .IsEqual(new[] { typeof(string), typeof(bool), typeof(int), typeof(IEnumerable<string>) });
    }

    [Fact]
    public void Interface_TargetNotGeneric_ReturnsTypeArguments()
    {
        // assert
        typeof(List<>).ResolveGenericArgumentsByImplementation(typeof(IEnumerable)).IsEqual(new[]
            { typeof(List<>).GetGenericArguments()[0] });
    }

    [Fact]
    public void Interface_NoImplementation_ReturnsNull()
    {
        // assert
        typeof(ValueTuple<,>).ResolveGenericArgumentsByImplementation(typeof(IEquatable<bool>))
            .IsDefault();
    }

    [Fact]
    public void Interface_WithImplementation_BuildArgs()
    {
        // assert
        typeof(BaseStruct<,,,>).ResolveGenericArgumentsByImplementation(
                typeof(IBase<string, int, bool, IEnumerable<string>>))!
            .IsEqual(new[] { typeof(string), typeof(int), typeof(bool), typeof(IEnumerable<string>) });
    }

    public struct BaseStruct<T1, T2, T3, T4> : IBase<T1, T2, T3, T4> where T1 : class where T2 : struct where T4 : IEnumerable<T1>
    {
    }

    private interface IBase<T1, T2, T3, T4> where T1 : class where T2 : struct where T4 : IEnumerable<T1>
    {
    }

    private struct StructEnumerable : IEnumerable
    {
        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

    private interface IClassConstraint<T> where T : class
    {
    }

    private interface INewConstraint<T> where T : new()
    {
    }

    private interface IEnumerableConstraint<T> where T : IEnumerable
    {
    }
}