using Annium.Net.Types.Tests.Lib.Mapper;
using Xunit;

namespace Annium.Net.Types.Tests.Mapper;

/// <summary>
/// Tests for enum type mapping functionality
/// </summary>
public class MapperEnumTests : MapperEnumTestsBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MapperEnumTests"/> class
    /// </summary>
    /// <param name="outputHelper">The test output helper</param>
    public MapperEnumTests(ITestOutputHelper outputHelper)
        : base(new TestProvider(), outputHelper) { }

    /// <summary>
    /// Tests mapping of enum types
    /// </summary>
    [Fact]
    public void Enum()
    {
        Enum_Base();
    }
}
