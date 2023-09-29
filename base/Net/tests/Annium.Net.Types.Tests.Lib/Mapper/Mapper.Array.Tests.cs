using System.Collections.Generic;
using Annium.Net.Types.Refs;
using Annium.Testing;
using Namotion.Reflection;
using Xunit.Abstractions;

namespace Annium.Net.Types.Tests.Lib.Mapper;

public abstract class MapperArrayTestsBase : TestBase
{
    protected MapperArrayTestsBase(
        ITestProvider testProvider,
        ITestOutputHelper outputHelper
    ) : base(testProvider, outputHelper)
    {
    }

    protected void Array_Base()
    {
        // arrange
        var target = typeof(int[]).ToContextualType();

        // act
        var modelRef = Map(target);

        // assert
        modelRef
            .As<ArrayRef>().Value
            .As<BaseTypeRef>().Name.Is(BaseType.Int);
        Models.IsEmpty();
    }

    protected void ArrayLike_Base()
    {
        // arrange
        var target = typeof(HashSet<>).ToContextualType();

        // act
        var modelRef = Map(target);

        // assert
        modelRef
            .As<ArrayRef>().Value
            .As<GenericParameterRef>().Name.Is("T");
        Models.IsEmpty();
    }
}