using Annium.Net.Types.Refs;
using Annium.Testing;
using Namotion.Reflection;
using Xunit.Abstractions;

namespace Annium.Net.Types.Tests.Lib.Mapper;

public abstract class MapperExcludedTestsBase : TestBase
{
    protected MapperExcludedTestsBase(
        ITestProvider testProvider,
        ITestOutputHelper outputHelper
    ) : base(testProvider, outputHelper)
    {
    }

    protected void Excluded_Base()
    {
        // arrange
        Config.Exclude(Match.Is(typeof(Sample)));
        var target = typeof(Sample).ToContextualType();

        // act
        var modelRef = Map(target).As<StructRef>();

        // assert
        modelRef.Namespace.Is(typeof(Sample).Namespace);
        modelRef.Name.Is(nameof(Sample));
        modelRef.Args.IsEmpty();
        Models.IsEmpty();
    }
}

file record Sample;