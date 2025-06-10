using Annium.Net.Types.Tests.Lib.Mapper;
using Xunit;

namespace Annium.Net.Types.Tests.Mapper;

/// <summary>
/// Tests for record type mapping functionality
/// </summary>
public class MapperRecordTests : MapperRecordTestsBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MapperRecordTests"/> class
    /// </summary>
    /// <param name="outputHelper">The test output helper</param>
    public MapperRecordTests(ITestOutputHelper outputHelper)
        : base(new TestProvider(), outputHelper) { }

    /// <summary>
    /// Tests mapping of interface-based record types
    /// </summary>
    [Fact]
    public void Interface()
    {
        Interface_Base();
    }

    /// <summary>
    /// Tests mapping of concrete implementation record types
    /// </summary>
    [Fact]
    public void Implementation()
    {
        Implementation_Base();
    }
}
