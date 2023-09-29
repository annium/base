using Annium.Net.Types.Tests.Lib.Mapper;
using Xunit;
using Xunit.Abstractions;

namespace Annium.Net.Types.Serialization.Json.Tests.Mapper;

public class MapperGenericParameterTests : MapperGenericParameterTestsBase
{
    public MapperGenericParameterTests(
        ITestOutputHelper outputHelper
    ) : base(
        new TestProvider(),
        outputHelper
    )
    {
    }

    [Fact]
    public void GenericParameter_NotNullable()
    {
        GenericParameter_NotNullable_Base();
    }

    [Fact]
    public void GenericParameter_Nullable()
    {
        GenericParameter_Nullable_Base();
    }
}