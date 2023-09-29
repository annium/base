using System;
using Annium.Net.Types.Extensions;
using Annium.Net.Types.Models;
using Annium.Net.Types.Refs;
using Annium.Testing;
using Namotion.Reflection;
using Xunit.Abstractions;

namespace Annium.Net.Types.Tests.Lib.Mapper;

public abstract class MapperInterfaceTestsBase : TestBase
{
    protected MapperInterfaceTestsBase(
        ITestProvider testProvider,
        ITestOutputHelper outputHelper
    ) : base(testProvider, outputHelper)
    {
    }

    protected void Empty_Base(Type type)
    {
        // arrange
        var target = type.ToContextualType();

        // act
        var modelRef = Map(target).As<InterfaceRef>();

        // assert
        modelRef.Namespace.Is(type.Namespace);
        modelRef.Name.Is(type.FriendlyName());
        modelRef.Args.IsEmpty();
        Models.Has(1);
        var model = Models.At(0).As<InterfaceModel>();
        model.Namespace.Is(type.GetNamespace());
        model.Name.Is(type.FriendlyName());
        model.Args.IsEmpty();
        model.Interfaces.IsEmpty();
        model.Fields.IsEmpty();
    }
}