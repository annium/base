using Annium.Net.Types.Tests.Lib.Mapper;
using Xunit;
using Xunit.Abstractions;

namespace Annium.Net.Types.Tests.Mapper;

public class MapperIncludedTests : MapperIncludedTestsBase
{
    public MapperIncludedTests(
        ITestOutputHelper outputHelper
    ) : base(
        new TestProvider(),
        outputHelper
    )
    {
    }

    [Fact]
    public void Included()
    {
        Included_Base();
    }
}