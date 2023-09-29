using System.Threading.Tasks;
using Annium.Net.Types.Refs;
using Annium.Testing;
using Namotion.Reflection;
using Xunit.Abstractions;

namespace Annium.Net.Types.Tests.Lib.Mapper;

public abstract class MapperSpecialTestsBase : TestBase
{
    protected MapperSpecialTestsBase(
        ITestProvider testProvider,
        ITestOutputHelper outputHelper
    ) : base(testProvider, outputHelper)
    {
    }

    protected void Task_Generic_Nullable_Base()
    {
        // arrange
        var target = typeof(Sample).ToContextualType().GetProperty(nameof(Sample.NullableTask))!.AccessorType;

        // act
        var modelRef = Map(target);

        // assert
        modelRef
            .As<PromiseRef>().Value
            .As<NullableRef>().Value
            .As<BaseTypeRef>().Name.Is(BaseType.String);
        Models.IsEmpty();
    }

    protected void Task_Generic_NotNullable_Base()
    {
        // arrange
        var target = typeof(Sample).ToContextualType().GetProperty(nameof(Sample.NotNullableTask))!.AccessorType;

        // act
        var modelRef = Map(target);

        // assert
        modelRef
            .As<PromiseRef>().Value
            .As<BaseTypeRef>().Name.Is(BaseType.String);
        Models.IsEmpty();
    }

    protected void Task_NonGeneric_Base()
    {
        // arrange
        var target = typeof(Task).ToContextualType();

        // act
        var modelRef = Map(target);

        // assert
        modelRef
            .As<PromiseRef>().Value
            .IsDefault();
        Models.IsEmpty();
    }
}

file record Sample(Task<string?> NullableTask, Task<string> NotNullableTask);