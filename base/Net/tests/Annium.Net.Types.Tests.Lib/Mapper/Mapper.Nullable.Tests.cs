using Annium.Net.Types.Refs;
using Annium.Testing;
using Namotion.Reflection;
using Xunit.Abstractions;

namespace Annium.Net.Types.Tests.Lib.Mapper;

public abstract class MapperNullableTestsBase : TestBase
{
    protected MapperNullableTestsBase(
        ITestProvider testProvider,
        ITestOutputHelper outputHelper
    ) : base(testProvider, outputHelper)
    {
    }

    protected void Nullable_BaseType_Struct_Base()
    {
        // arrange
        var target = typeof(int?).ToContextualType();

        // act
        var model = Map(target);

        // assert
        model
            .As<NullableRef>().Value
            .As<BaseTypeRef>().Name.Is(BaseType.Int);
        Models.IsEmpty();
    }

    protected void Nullable_BaseType_Class_Base()
    {
        // arrange
        var target = typeof(Sample).ToContextualType().GetProperty(nameof(Sample.Value))!.AccessorType;

        // act
        var model = Map(target);

        // assert
        model
            .As<NullableRef>().Value
            .As<BaseTypeRef>().Name.Is(BaseType.String);
        Models.IsEmpty();
    }
}

file record Sample(string? Value);