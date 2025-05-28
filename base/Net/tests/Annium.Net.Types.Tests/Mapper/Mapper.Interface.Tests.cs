using System;
using Annium.Net.Types.Tests.Lib.Mapper;
using Xunit;

namespace Annium.Net.Types.Tests.Mapper;

public class MapperInterfaceTests : MapperInterfaceTestsBase
{
    public MapperInterfaceTests(ITestOutputHelper outputHelper)
        : base(new TestProvider(), outputHelper) { }

    [Theory]
    [InlineData(typeof(IEmptyInterface))]
    public void Empty(Type type)
    {
        Empty_Base(type);
    }
}

file interface IEmptyInterface;
