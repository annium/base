using Annium.Core.DependencyInjection;
using Annium.Testing;
using Annium.Testing.Lib;
using Xunit;
using Xunit.Abstractions;

namespace Annium.Core.Mapper.Tests.Resolvers;

public class ConstructorMapResolverTest : TestBase
{
    public ConstructorMapResolverTest(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        Register(c => c.AddMapper(autoload: false));
    }

    [Fact]
    public void ConstructorMapping_Works()
    {
        // arrange
        var mapper = Get<IMapper>();
        var first = new A { Name = "first" };
        var second = new A { Name = "second" };

        // act
        var one = mapper.Map<B>(first);
        var arr = mapper.Map<B[]>(new[] { first, second });

        // assert
        one.Name.Is(first.Name);
        arr.Has(2);
        arr.At(0).Name.Is(first.Name);
        arr.At(1).Name.Is(second.Name);
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
}