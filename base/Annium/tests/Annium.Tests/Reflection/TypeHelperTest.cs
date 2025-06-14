using System;
using Annium.Reflection;
using Annium.Testing;
using Xunit;

namespace Annium.Tests.Reflection;

/// <summary>
/// Contains unit tests for the TypeHelper class.
/// </summary>
public class TypeHelperTest
{
    /// <summary>
    /// Verifies that GetAccessExpressions works correctly with multiple property access expressions.
    /// </summary>
    [Fact]
    public void GetAccessExpressions_Multiple_Works()
    {
        // arrange
        var data = new B
        {
            InnerOne = new A { One = "a", Two = "b" },
            InnerTwo = new A { One = "c", Two = "d" },
        };

        // act
        var expressions = TypeHelper.GetAccessExpressions<B>(x => new { x.InnerOne.One, x.InnerTwo });

        // assert
        expressions.Has(2);
        var getOne = expressions.At(0).Compile();
        var result = getOne.DynamicInvoke(data);
        result.Is(data.InnerOne.One);
        var getTwo = expressions.At(1).Compile();
        result = getTwo.DynamicInvoke(data);
        result.IsEqual(data.InnerTwo);
    }

    /// <summary>
    /// Verifies that GetAccessExpressions works correctly with a single property access expression.
    /// </summary>
    [Fact]
    public void GetAccessExpressions_Single_Works()
    {
        // arrange
        var data = new B
        {
            InnerOne = new A { One = "a", Two = "b" },
            InnerTwo = new A { One = "c", Two = "d" },
        };

        // act
        var expressions = TypeHelper.GetAccessExpressions<B>(x => x.InnerOne.One);

        // assert
        expressions.Has(1);
        var getOne = expressions.At(0).Compile();
        var result = getOne.DynamicInvoke(data);
        result.Is(data.InnerOne.One);
    }

    /// <summary>
    /// Verifies that ResolveProperties works correctly with multiple property access expressions.
    /// </summary>
    [Fact]
    public void ResolveProperties_Multiple_Works()
    {
        // act
        var properties = TypeHelper.ResolveProperties<B>(x => new { x.InnerOne.One, x.InnerTwo });

        // assert
        properties.Has(2);
        properties.At(0).Is(typeof(A).GetProperty(nameof(A.One)));
        properties.At(1).Is(typeof(B).GetProperty(nameof(B.InnerTwo)));
    }

    /// <summary>
    /// Verifies that ResolveProperty works correctly with a unary property access expression.
    /// </summary>
    [Fact]
    public void ResolveProperty_Unary_Works()
    {
        // assert
        TypeHelper.ResolveProperty<A>(x => x.Two).Is(typeof(A).GetProperty(nameof(A.Two)));
    }

    /// <summary>
    /// Verifies that ResolveProperty works correctly with a nested property access expression.
    /// </summary>
    [Fact]
    public void ResolveProperty_Inner_Works()
    {
        // assert
        TypeHelper.ResolveProperty<B>(x => x.InnerTwo.Two).Is(typeof(A).GetProperty(nameof(A.Two)));
    }

    /// <summary>
    /// Verifies that ResolveProperty throws ArgumentException when the expression is not a property access.
    /// </summary>
    [Fact]
    public void ResolveProperty_NotProperty_Fails()
    {
        // assert
        Wrap.It(() => TypeHelper.ResolveProperty<B>(x => x.InnerTwo.ToString()!)).Throws<ArgumentException>();
    }

    /// <summary>
    /// A test class with nested properties.
    /// </summary>
    private class B
    {
        /// <summary>
        /// Gets or sets the first inner property.
        /// </summary>
        public A InnerOne { get; set; } = null!;

        /// <summary>
        /// Gets or sets the second inner property.
        /// </summary>
        public A InnerTwo { get; set; } = null!;
    }

    /// <summary>
    /// A test class with string properties.
    /// </summary>
    private class A
    {
        /// <summary>
        /// Gets or sets the first string property.
        /// </summary>
        public string One { get; set; } = null!;

        /// <summary>
        /// Gets or sets the second string property.
        /// </summary>
        public string Two { get; set; } = null!;
    }
}
