using Annium.Net.Types.Tests.Lib.Mapper;
using Xunit;

namespace Annium.Net.Types.Serialization.Json.Tests.Mapper;

public class MapperBaseTypeTests : MapperBaseTypeTestsBase
{
    public MapperBaseTypeTests(ITestOutputHelper outputHelper)
        : base(new TestProvider(), outputHelper) { }

    [Fact]
    public void BaseType()
    {
        BaseType_Base();
    }
}
