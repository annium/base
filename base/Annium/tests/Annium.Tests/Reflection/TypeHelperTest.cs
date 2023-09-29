using System;
using Annium.Reflection;
using Annium.Testing;
using Xunit;

namespace Annium.Tests.Reflection;

public class TypeHelperTest
{
    [Fact]
    public void GetAccessExpressions_Multiple_Works()
    {
        // arrange
        var data = new B { InnerOne = new A { One = "a", Two = "b" }, InnerTwo = new A { One = "c", Two = "d" } };

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

    [Fact]
    public void GetAccessExpressions_Single_Works()
    {
        // arrange
        var data = new B { InnerOne = new A { One = "a", Two = "b" }, InnerTwo = new A { One = "c", Two = "d" } };

        // act
        var expressions = TypeHelper.GetAccessExpressions<B>(x => x.InnerOne.One);

        // assert
        expressions.Has(1);
        var getOne = expressions.At(0).Compile();
        var result = getOne.DynamicInvoke(data);
        result.Is(data.InnerOne.One);
    }

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

    [Fact]
    public void ResolveProperty_Unary_Works()
    {
        // assert
        TypeHelper.ResolveProperty<A>(x => x.Two).Is(typeof(A).GetProperty(nameof(A.Two)));
    }

    [Fact]
    public void ResolveProperty_Inner_Works()
    {
        // assert
        TypeHelper.ResolveProperty<B>(x => x.InnerTwo.Two).Is(typeof(A).GetProperty(nameof(A.Two)));
    }

    [Fact]
    public void ResolveProperty_NotProperty_Fails()
    {
        // assert
        Wrap.It(() => TypeHelper.ResolveProperty<B>(x => x.InnerTwo.ToString()!)).Throws<ArgumentException>();
    }

    private class B
    {
        public A InnerOne { get; set; } = null!;
        public A InnerTwo { get; set; } = null!;
    }

    private class A
    {
        public string One { get; set; } = null!;
        public string Two { get; set; } = null!;
    }
}