using Annium.Net.Types.Tests.Lib.Mapper;
using Xunit;

namespace Annium.Net.Types.Serialization.Json.Tests.Mapper;

/// <summary>
/// Tests for special type mapping functionality using JSON serialization
/// </summary>
public class MapperSpecialTests : MapperSpecialTestsBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MapperSpecialTests"/> class
    /// </summary>
    /// <param name="outputHelper">The test output helper</param>
    public MapperSpecialTests(ITestOutputHelper outputHelper)
        : base(new TestProvider(), outputHelper) { }

    /// <summary>
    /// Tests Task with generic nullable type mapping
    /// </summary>
    [Fact]
    public void Task_Generic_Nullable()
    {
        Task_Generic_Nullable_Base();
    }

    /// <summary>
    /// Tests Task with generic non-nullable type mapping
    /// </summary>
    [Fact]
    public void Task_Generic_NotNullable()
    {
        Task_Generic_NotNullable_Base();
    }

    /// <summary>
    /// Tests non-generic Task type mapping
    /// </summary>
    [Fact]
    public void Task_NonGeneric()
    {
        Task_NonGeneric_Base();
    }
}
