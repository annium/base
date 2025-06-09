using Annium.Net.Types.Tests.Lib.Mapper;
using Xunit;

namespace Annium.Net.Types.Serialization.Json.Tests.Mapper;

/// <summary>
/// Tests for enum type mapping functionality using JSON serialization
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
    /// Tests enum type mapping
    /// </summary>
    [Fact]
    public void Enum()
    {
        Enum_Base();
    }
}
