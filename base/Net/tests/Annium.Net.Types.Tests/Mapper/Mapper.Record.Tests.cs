using Annium.Net.Types.Tests.Lib.Mapper;
using Xunit;

namespace Annium.Net.Types.Tests.Mapper;

public class MapperRecordTests : MapperRecordTestsBase
{
    public MapperRecordTests(ITestOutputHelper outputHelper)
        : base(new TestProvider(), outputHelper) { }

    [Fact]
    public void Interface()
    {
        Interface_Base();
    }

    [Fact]
    public void Implementation()
    {
        Implementation_Base();
    }
}
