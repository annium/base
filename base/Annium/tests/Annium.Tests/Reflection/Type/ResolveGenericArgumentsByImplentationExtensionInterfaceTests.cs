using System;
using System.Collections;
using System.Collections.Generic;
using Annium.Reflection;
using Annium.Testing;
using Xunit;

namespace Annium.Tests.Reflection.Type;

public class ResolveGenericArgumentsByImplementationExtensionInterfaceTests
{
    [Fact]
    public void Param_ReferenceTypeConstraintFailure_ReturnsNull()
    {
        // assert
        typeof(IEnumerable<>)
            .ResolveGenericArgumentsByImplementation(typeof(IClassConstraint<>).GetGenericArguments()[0])
            .IsDefault();
    }

    [Fact]
    public void Param_StructTypeConstraintFailure_ReturnsNull()
    {
        // assert
        typeof(IEnumerable<>)
            .ResolveGenericArgumentsByImplementation(typeof(IStructConstraint<>).GetGenericArguments()[0])
            .IsDefault();
    }

    [Fact]
    public void Param_DefaultConstructorConstraintFailure_ReturnsNull()
    {
        // assert
        typeof(IEnumerable<>)
            .ResolveGenericArgumentsByImplementation(typeof(INewConstraint<>).GetGenericArguments()[0])
            .IsDefault();
    }

    [Fact]
    public void Param_ParameterConstraintFailure_ReturnsNull()
    {
        // assert
        typeof(IEnumerable<>)
            .ResolveGenericArgumentsByImplementation(typeof(IClassBaseConstraint<>).GetGenericArguments()[0])
            .IsDefault();
    }

    [Fact]
    public void Param_ParameterConstraintSuccess_ReturnsType()
    {
        // assert
        typeof(IEnumerable<>)
            .ResolveGenericArgumentsByImplementation(typeof(IEnumerableConstraint<>).GetGenericArguments()[0])
            .IsEqual(new[] { typeof(IEnumerable<>).GetGenericArguments()[0] });
    }

    [Fact]
    public void Interface_SameGenericDefinition_BuildsArgs()
    {
        // assert
        typeof(IEquatable<>).ResolveGenericArgumentsByImplementation(typeof(IEquatable<bool>))
            .IsEqual(new[] { typeof(bool) });
    }

    [Fact]
    public void Interface_NoImplementation_ReturnsNull()
    {
        // assert
        typeof(IEnumerable<>).ResolveGenericArgumentsByImplementation(typeof(IEquatable<bool>))
            .IsDefault();
    }

    [Fact]
    public void Interface_WithImplementation_BuildArgs()
    {
        // assert
        typeof(IParentOther<,>).ResolveGenericArgumentsByImplementation(
                typeof(IBase<string[], int, bool, IEnumerable<string[]>>))
            .IsEqual(new[] { typeof(string), typeof(int) });
    }

    private interface IParentOther<T1, T2> : IBase<T1[], T2, bool, IEnumerable<T1[]>> where T2 : struct
    {
    }

    private interface IBase<T1, T2, T3, T4> where T1 : class where T2 : struct where T4 : IEnumerable<T1>
    {
    }

    private interface IClassConstraint<T> where T : class
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

    private interface IEnumerableConstraint<T> where T : IEnumerable
    {
    }

    private interface IEquatableConstraint<T> where T : IEquatable<T>
    {
    }

    private class ClassBase
    {
    }
}