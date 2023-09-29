using System;
using System.Collections;
using System.IO;
using Annium.Reflection;
using Annium.Testing;
using Xunit;

namespace Annium.Tests.Reflection.Type;

public class ResolveGenericArgumentsByImplementationExtensionParameterTests
{
    [Fact]
    public void Param_ReferenceTypeConstraintFailure_ReturnsNull()
    {
        // assert
        typeof(IStructConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(IClassConstraint<>).GetGenericArguments()[0])
            .IsDefault();
    }

    [Fact]
    public void Param_StructTypeConstraintFailure_ReturnsNull()
    {
        // assert
        typeof(IClassConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(IStructConstraint<>).GetGenericArguments()[0])
            .IsDefault();
    }

    [Fact]
    public void Param_DefaultConstructorConstraintFailure_ReturnsNull()
    {
        // assert
        typeof(IClassConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(INewConstraint<>).GetGenericArguments()[0])
            .IsDefault();
    }

    [Fact]
    public void Param_ParameterConstraintFailure_ReturnsNull()
    {
        // assert
        typeof(IClassConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(IEnumerableConstraint<>).GetGenericArguments()[0])
            .IsDefault();
    }

    [Fact]
    public void Param_ParameterConstraintSuccess_ReturnsType()
    {
        // assert
        typeof(IClassBaseConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(IClassBaseConstraint<>).GetGenericArguments()[0])
            .IsEqual(new[] { typeof(IClassBaseConstraint<>).GetGenericArguments()[0] });
    }

    [Fact]
    public void Class_StructTypeConstraintFailure_ReturnsNull()
    {
        // assert
        typeof(IStructConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(string))
            .IsDefault();
    }

    [Fact]
    public void Class_DefaultConstructorConstraintFailure_ReturnsNull()
    {
        // assert
        typeof(INewConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(string))
            .IsDefault();
    }

    [Fact]
    public void Class_ParameterConstraintFailure_ReturnsNull()
    {
        // assert
        typeof(IEnumerableConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(FileInfo))
            .IsDefault();
    }

    [Fact]
    public void Class_ParameterConstraintSuccess_ReturnsType()
    {
        // assert
        typeof(RecurringBase<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(RecurringBase<RecurringDerived>))
            .IsEqual(new[] { typeof(RecurringBase<RecurringDerived>) });
    }

    [Fact]
    public void Struct_ReferenceTypeConstraintFailure_ReturnsNull()
    {
        // assert
        typeof(IClassConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(long))
            .IsDefault();
    }

    [Fact]
    public void Struct_StructTypeConstraintFailure_ReturnsNull()
    {
        // assert
        typeof(IStructConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(long?))
            .IsDefault();
    }

    [Fact]
    public void Struct_DefaultConstructorConstraintFailure_ReturnsNull()
    {
        // assert
        typeof(INewConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(ValueTuple<>))
            .IsDefault();
    }

    [Fact]
    public void Struct_ParameterConstraintFailure_ReturnsNull()
    {
        // assert
        typeof(IEnumerableConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(bool))
            .IsDefault();
    }

    [Fact]
    public void Interface_ReferenceTypeConstraintFailure_ReturnsNull()
    {
        // assert
        typeof(IClassConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(IEnumerable))
            .IsDefault();
    }

    [Fact]
    public void Interface_StructTypeConstraintFailure_ReturnsNull()
    {
        // assert
        typeof(IStructConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(IEnumerable))
            .IsDefault();
    }

    [Fact]
    public void Interface_DefaultConstructorConstraintFailure_ReturnsNull()
    {
        // assert
        typeof(INewConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(IEnumerable))
            .IsDefault();
    }

    [Fact]
    public void Interface_ParameterConstraintFailure_ReturnsNull()
    {
        // assert
        typeof(IEquatableConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(IEnumerable))
            .IsDefault();
    }

    [Fact]
    public void Interface_ParameterConstraintSuccess_ReturnsType()
    {
        // assert
        typeof(IEquatableConstraint<>).GetGenericArguments()[0]
            .ResolveGenericArgumentsByImplementation(typeof(IEquatable<string>))
            .IsEqual(new[] { typeof(IEquatable<string>) });
    }

    private class RecurringDerived : RecurringBase<RecurringDerived>
    {
    }

    private class RecurringBase<T> where T : RecurringBase<T>
    {
    }

    private interface IEquatableConstraint<T> where T : IEquatable<T>
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

    private class ClassBase
    {
    }
}