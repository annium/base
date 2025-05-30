using Annium.Net.Types.Tests.Lib.Mapper;
using Xunit;

namespace Annium.Net.Types.Tests.Mapper;

public class MapperNullableTests : MapperNullableTestsBase
{
    public MapperNullableTests(ITestOutputHelper outputHelper)
        : base(new TestProvider(), outputHelper) { }

    [Fact]
    public void Nullable_BaseType_Struct()
    {
        Nullable_BaseType_Struct_Base();
    }

    [Fact]
    public void Nullable_BaseType_Class()
    {
        Nullable_BaseType_Class_Base();
    }
}
