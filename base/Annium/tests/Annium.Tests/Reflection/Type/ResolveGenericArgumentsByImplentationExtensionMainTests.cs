using System;
using Annium.Reflection;
using Annium.Testing;
using Xunit;

namespace Annium.Tests.Reflection.Type;

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
        Wrap.It(() => (null as System.Type)!.ResolveGenericArgumentsByImplementation(typeof(bool)))
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
