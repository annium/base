using System.Collections.Generic;
using System.Linq;
using Annium.Core.DependencyInjection;
using Annium.Testing;
using Annium.Testing.Lib;
using Xunit;
using Xunit.Abstractions;

namespace Annium.Core.Mapper.Tests.Resolvers;

public class EnumerableMapResolverTest : TestBase
{
    public EnumerableMapResolverTest(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        Register(c => c.AddMapper(autoload: false));
    }

    [Fact]
    public void ToArray_Works()
    {
        // arrange
        var mapper = Get<IMapper>();
        var value = new[] { new A { Name = "name" } };

        // act
        var one = mapper.Map<B[]>(value);
        var two = mapper.Map<C[]>(value);

        // assert
        one.Has(1);
        one.At(0).Name.Is(value[0].Name);
        two.Has(1);
        two.At(0).Name.Is(value[0].Name);
    }

    [Fact]
    public void ToCollection_Works()
    {
        // arrange
        var mapper = Get<IMapper>();
        var value = new[] { new A { Name = "name" } };

        // act
        var result = mapper.Map<List<B>>(value);

        // assert
        result.Has(1);
        result.At(0).Name.Is(value[0].Name);
    }

    [Fact]
    public void ToDictionary_Works()
    {
        // arrange
        var mapper = Get<IMapper>();
        var value = new Dictionary<string, A> { { "one", new A { Name = "name" } } };

        // act
        IDictionary<string, B> result = mapper.Map<Dictionary<string, B>>(value);

        // assert
        result.Has(1);
        result.At("one").Name.Is(value["one"].Name);
    }

    [Fact]
    public void ToIEnumerable_Works()
    {
        // arrange
        var mapper = Get<IMapper>();
        var value = new[] { new A { Name = "name" } };

        // act
        var result = mapper.Map<IEnumerable<B>>(value).ToArray();

        // assert
        result.Has(1);
        result.At(0).Name.Is(value[0].Name);
    }

    [Fact]
    public void ToIDictionary_Works()
    {
        // arrange
        var mapper = Get<IMapper>();
        var value = new Dictionary<string, A> { { "one", new A { Name = "name" } } };

        // act
        var result = mapper.Map<IReadOnlyDictionary<string, B>>(value);

        // assert
        result.Has(1);
        result.At("one").Name.Is(value["one"].Name);
    }

    [Fact]
    public void ToIDictionary_Same_Works()
    {
        // arrange
        var mapper = Get<IMapper>();
        var value = new Dictionary<string, A> { { "one", new A { Name = "name" } } };

        // act
        var result = mapper.Map<IReadOnlyDictionary<string, A>>(value);

        // assert
        result.Has(1);
        result.At("one").Name.Is(value["one"].Name);
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

    private class C
    {
        public string? Name { get; set; }
    }
}