using Annium.Net.Types.Tests.Lib.Mapper;
using Xunit;

namespace Annium.Net.Types.Tests.Mapper;

/// <summary>
/// Tests for generic parameter type mapping functionality
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
    /// Tests mapping of non-nullable generic parameters
    /// </summary>
    [Fact]
    public void GenericParameter_NotNullable()
    {
        GenericParameter_NotNullable_Base();
    }

    /// <summary>
    /// Tests mapping of nullable generic parameters
    /// </summary>
    [Fact]
    public void GenericParameter_Nullable()
    {
        GenericParameter_Nullable_Base();
    }
}
