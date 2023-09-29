using Annium.Core.DependencyInjection;
using Annium.Testing;
using Annium.Testing.Lib;
using Xunit;
using Xunit.Abstractions;

namespace Annium.Core.Mapper.Tests;

public class BaseTest : TestBase
{
    public BaseTest(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        Register(c => c.AddMapper(autoload: false));
    }

    [Fact]
    public void SameType_ReturnsSource()
    {
        // arrange
        var mapper = Get<IMapper>();
        var value = new A { Name = "name" };

        // act
        var result = mapper.Map<A>(value);

        // assert
        result.IsEqual(value);
    }

    [Fact]
    public void Nesting_Works()
    {
        // arrange
        var mapper = Get<IMapper>();
        var value = new D(new A { Name = "name" }, "nice");

        // act
        var result = mapper.Map<E>(value);

        // assert
        result.Inner!.Name.Is(value.Inner!.Name);
        result.Value.Is(value.Value);
    }

    [Fact]
    public void NullableNesting_Works()
    {
        // arrange
        var mapper = Get<IMapper>();
        var value = new D(null, "nice");

        // act
        var result = mapper.Map<E>(value);

        // assert
        result.Inner.IsDefault();
        result.Value.Is(value.Value);
    }

    private class A
    {
        public string? Name { get; set; }
    }

    private class B
    {
        public string? Name { get; }

        public B(string? name)
        {
            Name = name;
        }
    }

    private class D
    {
        public A? Inner { get; }

        public string? Value { get; }

        public D(A? inner, string? value)
        {
            Inner = inner;
            Value = value;
        }
    }

    private class E
    {
        public B? Inner { get; set; }

        public string? Value { get; set; }
    }
}