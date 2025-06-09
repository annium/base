using Annium.Net.Types.Tests.Lib.Mapper;
using Xunit;

namespace Annium.Net.Types.Serialization.Json.Tests.Mapper;

/// <summary>
/// Tests for array type mapping functionality using JSON serialization
/// </summary>
public class MapperArrayTests : MapperArrayTestsBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MapperArrayTests"/> class
    /// </summary>
    /// <param name="outputHelper">The test output helper</param>
    public MapperArrayTests(ITestOutputHelper outputHelper)
        : base(new TestProvider(), outputHelper) { }

    /// <summary>
    /// Tests array type mapping
    /// </summary>
    [Fact]
    public void Array()
    {
        Array_Base();
    }

    /// <summary>
    /// Tests array-like type mapping
    /// </summary>
    [Fact]
    public void ArrayLike()
    {
        ArrayLike_Base();
    }
}
