using System;
using System.Collections.Generic;
using Annium.Core.Runtime.Types;
using Annium.Testing;
using Annium.Testing.Lib;
using Xunit;
using Xunit.Abstractions;

namespace Annium.Core.Runtime.Tests.Types;

public class TypeIdTests : TestBase
{
    public TypeIdTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void PlainId_Works()
    {
        Assert(typeof(int));
    }

    [Fact]
    public void ConstructedGenericId_Works()
    {
        Assert(typeof(Dictionary<string, List<int>>));
    }

    [Fact]
    public void OpenGenericId_Works()
    {
        Assert(typeof(Dictionary<,>));
    }

    private void Assert(Type type)
    {
        // act
        var id = type.GetTypeId();

        // assert
        id.Id.Contains(type.Namespace!).IsTrue();
        id.Id.Contains(type.Name).IsTrue();
        (id == type.GetTypeId()).IsTrue();
        var tm = Get<ITypeManager>();
        var parsed = TypeId.TryParse(id.Id, tm);
        (parsed == id).IsTrue();
        parsed!.Type.Is(type);
    }
}