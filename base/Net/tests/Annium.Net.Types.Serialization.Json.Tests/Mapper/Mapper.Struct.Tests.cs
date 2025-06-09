using System;
using Annium.Net.Types.Tests.Lib.Mapper;
using Xunit;

namespace Annium.Net.Types.Serialization.Json.Tests.Mapper;

/// <summary>
/// Tests for struct type mapping functionality using JSON serialization
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
    /// Tests empty struct and record mapping
    /// </summary>
    /// <param name="type">The type to test</param>
    [Theory]
    [InlineData(typeof(EmptyStruct))]
    [InlineData(typeof(EmptyRecord))]
    public void Empty(Type type)
    {
        Empty_Base(type);
    }

    /// <summary>
    /// Tests struct type mapping
    /// </summary>
    [Fact]
    public void Struct()
    {
        Struct_Base();
    }
}

/// <summary>
/// An empty struct for testing purposes
/// </summary>
file struct EmptyStruct;

/// <summary>
/// An empty record for testing purposes
/// </summary>
file record EmptyRecord;
