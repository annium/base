using Annium.Net.Types.Tests.Lib.Mapper;
using Xunit;

namespace Annium.Net.Types.Serialization.Json.Tests.Mapper;

/// <summary>
/// Tests for generic parameter type mapping functionality using JSON serialization
/// </summary>
public class MapperGenericParameterTests : MapperGenericParameterTestsBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MapperGenericParameterTests"/> class
    /// </summary>
    /// <param name="outputHelper">The test output helper</param>
    public MapperGenericParameterTests(ITestOutputHelper outputHelper)
        : base(new TestProvider(), outputHelper) { }

    /// <summary>
    /// Tests generic parameter mapping for non-nullable types
    /// </summary>
    [Fact]
    public void GenericParameter_NotNullable()
    {
        GenericParameter_NotNullable_Base();
    }

    /// <summary>
    /// Tests generic parameter mapping for nullable types
    /// </summary>
    [Fact]
    public void GenericParameter_Nullable()
    {
        GenericParameter_Nullable_Base();
    }
}
