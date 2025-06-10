using Annium.Net.Types.Tests.Lib.Mapper;
using Xunit;

namespace Annium.Net.Types.Tests.Mapper;

/// <summary>
/// Tests for nullable type mapping functionality
/// </summary>
public class MapperNullableTests : MapperNullableTestsBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MapperNullableTests"/> class
    /// </summary>
    /// <param name="outputHelper">The test output helper</param>
    public MapperNullableTests(ITestOutputHelper outputHelper)
        : base(new TestProvider(), outputHelper) { }

    /// <summary>
    /// Tests mapping of nullable value types (structs)
    /// </summary>
    [Fact]
    public void Nullable_BaseType_Struct()
    {
        Nullable_BaseType_Struct_Base();
    }

    /// <summary>
    /// Tests mapping of nullable reference types (classes)
    /// </summary>
    [Fact]
    public void Nullable_BaseType_Class()
    {
        Nullable_BaseType_Class_Base();
    }
}
