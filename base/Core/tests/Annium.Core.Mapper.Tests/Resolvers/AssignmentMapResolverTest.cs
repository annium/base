using Annium.Core.DependencyInjection;
using Annium.Testing;
using Annium.Testing.Lib;
using Xunit;
using Xunit.Abstractions;

namespace Annium.Core.Mapper.Tests.Resolvers;

public class AssignmentMapResolverTest : TestBase
{
    public AssignmentMapResolverTest(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        Register(c => c.AddMapper(autoload: false));
    }

    [Fact]
    public void AssignmentMapping_Works()
    {
        // arrange
        var mapper = Get<IMapper>();
        var value = new A { Name = "name" };

        // act
        var result = mapper.Map<B>(value);

        // assert
        result.Name.Is(value.Name);
    }

    [Fact]
    public void AssignmentMapping_WithExcessProperties_Works()
    {
        // arrange
        var mapper = Get<IMapper>();
        var value = new A { Name = "name" };

        // act
        var result = mapper.Map<C>(value);

        // assert
        result.IsNotDefault();
    }

    private class A
    {
        public string? Name { get; set; }
    }

    private class B
    {
        public string? Name { get; set; }
    }

    private class C
    {
    }
}