using Annium.Core.DependencyInjection;
using Annium.Testing;
using Annium.Testing.Lib;
using Xunit;
using Xunit.Abstractions;

namespace Annium.Core.Mapper.Tests.Resolvers;

public class InstanceOfMapResolverTest : TestBase
{
    public InstanceOfMapResolverTest(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        Register(c => c.AddMapper(autoload: false));
    }

    [Fact]
    public void InstanceOf_Works()
    {
        // arrange
        var mapper = Get<IMapper>();
        var value = new Payload();

        // act
        var result = mapper.Map<Payload>(value);

        // assert
        result.Is(value);
    }

    private class Payload
    {
    }
}