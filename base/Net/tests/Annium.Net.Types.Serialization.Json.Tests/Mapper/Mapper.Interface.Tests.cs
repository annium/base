using System;
using Annium.Net.Types.Tests.Lib.Mapper;
using Xunit;
using Xunit.Abstractions;

namespace Annium.Net.Types.Serialization.Json.Tests.Mapper;

public class MapperInterfaceTests : MapperInterfaceTestsBase
{
    public MapperInterfaceTests(
        ITestOutputHelper outputHelper
    ) : base(
        new TestProvider(),
        outputHelper
    )
    {
    }

    [Theory]
    [InlineData(typeof(IEmptyInterface))]
    public void Empty(Type type)
    {
        Empty_Base(type);
    }
}

file interface IEmptyInterface
{
}