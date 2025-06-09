using System;
using Annium.Net.Types.Tests.Lib.Mapper;
using Xunit;

namespace Annium.Net.Types.Serialization.Json.Tests.Mapper;

/// <summary>
/// Tests for interface type mapping functionality using JSON serialization
/// </summary>
public class MapperInterfaceTests : MapperInterfaceTestsBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MapperInterfaceTests"/> class
    /// </summary>
    /// <param name="outputHelper">The test output helper</param>
    public MapperInterfaceTests(ITestOutputHelper outputHelper)
        : base(new TestProvider(), outputHelper) { }

    /// <summary>
    /// Tests empty interface mapping
    /// </summary>
    /// <param name="type">The interface type to test</param>
    [Theory]
    [InlineData(typeof(IEmptyInterface))]
    public void Empty(Type type)
    {
        Empty_Base(type);
    }
}

/// <summary>
/// An empty interface for testing purposes
/// </summary>
file interface IEmptyInterface;
