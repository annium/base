using Annium.Net.Types.Extensions;
using Annium.Net.Types.Models;
using Annium.Net.Types.Refs;
using Annium.Testing;
using Namotion.Reflection;
using Xunit.Abstractions;

namespace Annium.Net.Types.Tests.Lib.Mapper;

public abstract class MapperIncludedTestsBase : TestBase
{
    protected MapperIncludedTestsBase(
        ITestProvider testProvider,
        ITestOutputHelper outputHelper
    ) : base(testProvider, outputHelper)
    {
    }

    protected void Included_Base()
    {
        // arrange
        Config.Include(typeof(Sample));
        var target = typeof(int).ToContextualType();

        // act
        var modelRef = Map(target).As<BaseTypeRef>();

        // assert
        modelRef.Name.Is(BaseType.Int);
        Models.Has(1);
        var sample = Models.At(0).As<StructModel>();
        sample.Namespace.Is(typeof(Sample).Namespace!.ToNamespace());
        sample.Name.Is(nameof(Sample));
    }
}

file record Sample;