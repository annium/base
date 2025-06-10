using Annium.Net.Types.Tests.Lib.Mapper;
using Xunit;

namespace Annium.Net.Types.Tests.Mapper;

/// <summary>
/// Tests for special type mapping functionality, particularly Task types
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
    /// Tests mapping of generic Task types with nullable return values
    /// </summary>
    [Fact]
    public void Task_Generic_Nullable()
    {
        Task_Generic_Nullable_Base();
    }

    /// <summary>
    /// Tests mapping of generic Task types with non-nullable return values
    /// </summary>
    [Fact]
    public void Task_Generic_NotNullable()
    {
        Task_Generic_NotNullable_Base();
    }

    /// <summary>
    /// Tests mapping of non-generic Task types
    /// </summary>
    [Fact]
    public void Task_NonGeneric()
    {
        Task_NonGeneric_Base();
    }
}
