using Annium.Net.Types.Tests.Lib.Mapper;
using Xunit;
using Xunit.Abstractions;

namespace Annium.Net.Types.Tests.Mapper;

public class MapperArrayTests : MapperArrayTestsBase
{
    public MapperArrayTests(
        ITestOutputHelper outputHelper
    ) : base(
        new TestProvider(),
        outputHelper
    )
    {
    }

    [Fact]
    public void Array()
    {
        Array_Base();
    }

    [Fact]
    public void ArrayLike()
    {
        ArrayLike_Base();
    }
}