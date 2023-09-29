using Annium.Net.Types.Refs;
using Annium.Testing;
using Namotion.Reflection;
using Xunit.Abstractions;

namespace Annium.Net.Types.Tests.Lib.Mapper;

public abstract class MapperBaseTypeTestsBase : TestBase
{
    protected MapperBaseTypeTestsBase(
        ITestProvider testProvider,
        ITestOutputHelper outputHelper
    ) : base(testProvider, outputHelper)
    {
    }

    protected void BaseType_Base()
    {
        // arrange
        var target = typeof(int).ToContextualType();

        // act
        var modelRef = Map(target).As<BaseTypeRef>();

        // assert
        modelRef.Name.Is(BaseType.Int);
        Models.IsEmpty();
    }
}