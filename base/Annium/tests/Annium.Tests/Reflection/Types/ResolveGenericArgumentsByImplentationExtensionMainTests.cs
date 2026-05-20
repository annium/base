using System;
using Annium.Reflection;
using Annium.Testing;
using Xunit;

namespace Annium.Tests.Reflection.Types;

/// <summary>
/// Contains unit tests for the ResolveGenericArgumentsByImplementation extension method.
/// </summary>
public class ResolveGenericArgumentsByImplementationExtensionMainTests
{
    /// <summary>
    /// Verifies that ResolveGenericArgumentsByImplementation returns empty types when type is not generic.
    /// </summary>
    [Fact]
    public void TypeNull_Throws()
    {
        // assert
        Wrap.It(() => (null as Type)!.ResolveGenericArgumentsByImplementation(typeof(bool)))
            .Throws<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that ResolveGenericArgumentsByImplementation returns type arguments when type is generic and defined.
    /// </summary>
    [Fact]
    public void TargetNull_Throws()
    {
        // assert
        Wrap.It(() => typeof(bool).ResolveGenericArgumentsByImplementation(null!)).Throws<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that ResolveGenericArgumentsByImplementation returns type arguments when type is generic and defined.
    /// </summary>
    [Fact]
    public void BuildArgs_InferByDefinitions()
    {
        // assert
        typeof(ConstrainedComplex<,,,>)
            .ResolveGenericArgumentsByImplementation(typeof(IGeneric<IGeneric<bool, IGeneric<bool, int>>>))!
            .IsEqual(
                new[]
                {
                    typeof(IGeneric<bool, IGeneric<bool, int>>),
                    typeof(bool),
                    typeof(IGeneric<bool, int>),
                    typeof(int),
                }
            );
    }

    /// <summary>
    /// Interface→class arm: when the receiver is an interface and the target is a class,
    /// <c>ResolveInterfaceArgumentsByTarget</c> returns <c>null</c> (interfaces don't extend classes).
    /// </summary>
    [Fact]
    public void Interface_TargetIsClass_ReturnsNull()
    {
        var result = typeof(IGeneric<bool>).ResolveGenericArgumentsByImplementation(typeof(SimpleClass));
        (result is null).IsTrue();
    }

    /// <summary>
    /// Interface→value-type arm: same idea — interfaces don't implement value types.
    /// </summary>
    [Fact]
    public void Interface_TargetIsValueType_ReturnsNull()
    {
        var result = typeof(IGeneric<bool>).ResolveGenericArgumentsByImplementation(typeof(int));
        (result is null).IsTrue();
    }

    /// <summary>
    /// Class→value-type arm: <c>ResolveClassArgumentsByTarget</c> returns <c>null</c> when the
    /// target is a value type (a class cannot inherit a struct).
    /// </summary>
    [Fact]
    public void Class_TargetIsValueType_ReturnsNull()
    {
        var result = typeof(SimpleClass).ResolveGenericArgumentsByImplementation(typeof(int));
        (result is null).IsTrue();
    }

    /// <summary>
    /// Struct→class arm: <c>ResolveStructArgumentsByTarget</c> returns <c>null</c> when the
    /// target is a class (a struct cannot inherit a class).
    /// </summary>
    [Fact]
    public void Struct_TargetIsClass_ReturnsNull()
    {
        var result = typeof(int).ResolveGenericArgumentsByImplementation(typeof(SimpleClass));
        (result is null).IsTrue();
    }

    /// <summary>
    /// A non-generic concrete class used as a target in the negative arms above.
    /// </summary>
    private class SimpleClass;

    /// <summary>
    /// Represents a constrained complex class for testing generic argument resolution.
    /// </summary>
    /// <typeparam name="T1">The first type parameter, must implement IGeneric&lt;T2, T3&gt;.</typeparam>
    /// <typeparam name="T2">The second type parameter.</typeparam>
    /// <typeparam name="T3">The third type parameter, must implement IGeneric&lt;T2, T4&gt;.</typeparam>
    /// <typeparam name="T4">The fourth type parameter.</typeparam>
    private class ConstrainedComplex<T1, T2, T3, T4> : IGeneric<T1>
        where T1 : IGeneric<T2, T3>
        where T3 : IGeneric<T2, T4>;

    /// <summary>
    /// Represents a generic interface with one type parameter.
    /// </summary>
    /// <typeparam name="T">The type parameter.</typeparam>
    private interface IGeneric<T>;

    /// <summary>
    /// Represents a generic interface with two type parameters.
    /// </summary>
    /// <typeparam name="T1">The first type parameter.</typeparam>
    /// <typeparam name="T2">The second type parameter.</typeparam>
    private interface IGeneric<T1, T2>;
}
