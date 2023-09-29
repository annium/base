using Annium.Net.Types.Extensions;
using Annium.Net.Types.Models;
using Annium.Net.Types.Refs;
using Annium.Testing;
using Namotion.Reflection;
using Xunit.Abstractions;

namespace Annium.Net.Types.Tests.Lib.Mapper;

public abstract class MapperEnumTestsBase : TestBase
{
    protected MapperEnumTestsBase(
        ITestProvider testProvider,
        ITestOutputHelper outputHelper
    ) : base(testProvider, outputHelper)
    {
    }

    protected void Enum_Base()
    {
        // arrange
        var target = typeof(Sample).ToContextualType();

        // act
        var modelRef = Map(target).As<EnumRef>();

        // assert
        modelRef.Namespace.Is(typeof(Sample).GetNamespace().ToString());
        modelRef.Name.Is(nameof(Sample));
        Models.Has(1);
        var model = Models.At(0).As<EnumModel>();
        model.Namespace.Is(typeof(Sample).GetNamespace());
        model.Name.Is(nameof(Sample));
        model.Values.Has(2);
        model.Values.At("A").Is(1);
        model.Values.At("B").Is(3);
    }
}

file enum Sample
{
    A = 1,
    B = 3
}