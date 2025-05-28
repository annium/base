using Annium.Net.Types.Tests.Lib.Mapper;
using Xunit;

namespace Annium.Net.Types.Tests.Mapper;

public class MapperEnumTests : MapperEnumTestsBase
{
    public MapperEnumTests(ITestOutputHelper outputHelper)
        : base(new TestProvider(), outputHelper) { }

    [Fact]
    public void Enum()
    {
        Enum_Base();
    }
}
