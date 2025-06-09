using Annium.Net.Types.Tests.Lib.Mapper;
using Xunit;

namespace Annium.Net.Types.Serialization.Json.Tests.Mapper;

/// <summary>
/// Tests for included type mapping functionality using JSON serialization
/// </summary>
public class MapperIncludedTests : MapperIncludedTestsBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MapperIncludedTests"/> class
    /// </summary>
    /// <param name="outputHelper">The test output helper</param>
    public MapperIncludedTests(ITestOutputHelper outputHelper)
        : base(new TestProvider(), outputHelper) { }

    /// <summary>
    /// Tests included type mapping
    /// </summary>
    [Fact]
    public void Included()
    {
        Included_Base();
    }
}
