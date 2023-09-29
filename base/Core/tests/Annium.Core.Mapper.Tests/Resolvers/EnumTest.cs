using Annium.Core.DependencyInjection;
using Annium.Core.Mapper.Attributes;
using Annium.Testing;
using Annium.Testing.Lib;
using Xunit;
using Xunit.Abstractions;

namespace Annium.Core.Mapper.Tests.Resolvers;

public class EnumTest : TestBase
{
    public EnumTest(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        Register(c => c.AddMapper(autoload: false));
    }

    [Fact]
    public void EnumMapping_Works()
    {
        // arrange
        var mapper = Get<IMapper>();

        // assert
        mapper.Map<string>(Sex.Male).Is("Male");
        mapper.Map<Sex>("female").Is(Sex.Female);
    }

    [AutoMapped]
    private enum Sex
    {
        Male,
        Female
    }
}