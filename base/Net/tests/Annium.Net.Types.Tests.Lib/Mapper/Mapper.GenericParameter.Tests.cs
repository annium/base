using System.Collections.Generic;
using Annium.Net.Types.Refs;
using Annium.Testing;
using Namotion.Reflection;
using Xunit.Abstractions;

namespace Annium.Net.Types.Tests.Lib.Mapper;

public abstract class MapperGenericParameterTestsBase : TestBase
{
    protected MapperGenericParameterTestsBase(
        ITestProvider testProvider,
        ITestOutputHelper outputHelper
    ) : base(testProvider, outputHelper)
    {
    }

    protected void GenericParameter_NotNullable_Base()
    {
        // arrange
        var target = typeof(List<>).GetGenericArguments()[0].ToContextualType();

        // act
        var modelRef = Map(target);

        // assert
        modelRef
            .As<GenericParameterRef>().Name.Is(target.Type.Name);
        Models.IsEmpty();
    }

    protected void GenericParameter_Nullable_Base()
    {
        // arrange
        var target = typeof(Sample<>).ToContextualType().GetProperty(nameof(Sample<string>.Value))!.AccessorType;

        // act
        var modelRef = Map(target);

        // assert
        modelRef
            .As<NullableRef>().Value
            .As<GenericParameterRef>().Name.Is(target.Type.Name);
        Models.IsEmpty();
    }
}

file record Sample<T>(T? Value) where T : notnull;