using Annium.Net.Types.Tests.Lib.Mapper;
using Xunit;

namespace Annium.Net.Types.Serialization.Json.Tests.Mapper;

public class MapperExcludedTests : MapperExcludedTestsBase
{
    public MapperExcludedTests(ITestOutputHelper outputHelper)
        : base(new TestProvider(), outputHelper) { }

    [Fact]
    public void Excluded()
    {
        Excluded_Base();
    }
}
