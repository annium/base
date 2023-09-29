using Annium.Net.Types.Tests.Lib.Mapper;
using Xunit;
using Xunit.Abstractions;

namespace Annium.Net.Types.Tests.Mapper;

public class MapperBaseTypeTests : MapperBaseTypeTestsBase
{
    public MapperBaseTypeTests(
        ITestOutputHelper outputHelper
    ) : base(
        new TestProvider(),
        outputHelper
    )
    {
    }

    [Fact]
    public void BaseType()
    {
        BaseType_Base();
    }
}