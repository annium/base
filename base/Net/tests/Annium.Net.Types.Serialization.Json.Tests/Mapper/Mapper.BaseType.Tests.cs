using Annium.Net.Types.Tests.Lib.Mapper;
using Xunit;

namespace Annium.Net.Types.Serialization.Json.Tests.Mapper;

/// <summary>
/// Tests for base type mapping functionality using JSON serialization
/// </summary>
public class MapperBaseTypeTests : MapperBaseTypeTestsBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MapperBaseTypeTests"/> class
    /// </summary>
    /// <param name="outputHelper">The test output helper</param>
    public MapperBaseTypeTests(ITestOutputHelper outputHelper)
        : base(new TestProvider(), outputHelper) { }

    /// <summary>
    /// Tests base type mapping
    /// </summary>
    [Fact]
    public void BaseType()
    {
        BaseType_Base();
    }
}
