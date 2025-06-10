using Annium.Net.Types.Tests.Lib.Mapper;
using Xunit;

namespace Annium.Net.Types.Serialization.Json.Tests.Mapper;

/// <summary>
/// Tests for nullable type mapping functionality using JSON serialization
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
    /// Tests nullable struct base type mapping
    /// </summary>
    [Fact]
    public void Nullable_BaseType_Struct()
    {
        Nullable_BaseType_Struct_Base();
    }

    /// <summary>
    /// Tests nullable class base type mapping
    /// </summary>
    [Fact]
    public void Nullable_BaseType_Class()
    {
        Nullable_BaseType_Class_Base();
    }
}
