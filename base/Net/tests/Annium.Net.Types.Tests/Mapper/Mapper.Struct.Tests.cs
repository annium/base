using System;
using Annium.Net.Types.Tests.Lib.Mapper;
using Xunit;

namespace Annium.Net.Types.Tests.Mapper;

/// <summary>
/// Tests for struct type mapping functionality
/// </summary>
public class MapperStructTests : MapperStructTestsBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MapperStructTests"/> class
    /// </summary>
    /// <param name="outputHelper">The test output helper</param>
    public MapperStructTests(ITestOutputHelper outputHelper)
        : base(new TestProvider(), outputHelper) { }

    /// <summary>
    /// Tests mapping of empty struct types
    /// </summary>
    /// <param name="type">The struct type to test</param>
    [Theory]
    [InlineData(typeof(EmptyStruct))]
    [InlineData(typeof(EmptyRecord))]
    public void Empty(Type type)
    {
        Empty_Base(type);
    }

    /// <summary>
    /// Tests mapping of complex struct types
    /// </summary>
    [Fact]
    public void Struct()
    {
        Struct_Base();
    }
}

/// <summary>
/// Empty struct for testing struct mapping
/// </summary>
file struct EmptyStruct;

/// <summary>
/// Empty record for testing struct mapping
/// </summary>
file record EmptyRecord;
