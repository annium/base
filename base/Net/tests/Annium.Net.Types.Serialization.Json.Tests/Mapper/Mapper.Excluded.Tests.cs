using Annium.Net.Types.Tests.Lib.Mapper;
using Xunit;

namespace Annium.Net.Types.Serialization.Json.Tests.Mapper;

/// <summary>
/// Tests for excluded type mapping functionality using JSON serialization
/// </summary>
public class MapperExcludedTests : MapperExcludedTestsBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MapperExcludedTests"/> class
    /// </summary>
    /// <param name="outputHelper">The test output helper</param>
    public MapperExcludedTests(ITestOutputHelper outputHelper)
        : base(new TestProvider(), outputHelper) { }

    /// <summary>
    /// Tests excluded type mapping
    /// </summary>
    [Fact]
    public void Excluded()
    {
        Excluded_Base();
    }
}
