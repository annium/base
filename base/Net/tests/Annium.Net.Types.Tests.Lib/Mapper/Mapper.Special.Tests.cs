using System.Threading.Tasks;
using Annium.Net.Types.Refs;
using Annium.Testing;
using Annium.Testing.Collection;
using Namotion.Reflection;
using Xunit;

namespace Annium.Net.Types.Tests.Lib.Mapper;

/// <summary>
/// Base class for testing special type mapping functionality, particularly for Task types
/// </summary>
public abstract class MapperSpecialTestsBase : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MapperSpecialTestsBase"/> class
    /// </summary>
    /// <param name="testProvider">The test provider for type mapping operations</param>
    /// <param name="outputHelper">The test output helper</param>
    protected MapperSpecialTestsBase(ITestProvider testProvider, ITestOutputHelper outputHelper)
        : base(testProvider, outputHelper) { }

    /// <summary>
    /// Tests mapping of generic Task types with nullable return values
    /// </summary>
    protected void Task_Generic_Nullable_Base()
    {
        // arrange
        var target = typeof(Sample).ToContextualType().GetProperty(nameof(Sample.NullableTask))!.AccessorType;

        // act
        var modelRef = Map(target);

        // assert
        modelRef.As<PromiseRef>().Value.As<NullableRef>().Value.As<BaseTypeRef>().Name.Is(BaseType.String);
        Models.IsEmpty();
    }

    /// <summary>
    /// Tests mapping of generic Task types with non-nullable return values
    /// </summary>
    protected void Task_Generic_NotNullable_Base()
    {
        // arrange
        var target = typeof(Sample).ToContextualType().GetProperty(nameof(Sample.NotNullableTask))!.AccessorType;

        // act
        var modelRef = Map(target);

        // assert
        modelRef.As<PromiseRef>().Value.As<BaseTypeRef>().Name.Is(BaseType.String);
        Models.IsEmpty();
    }

    /// <summary>
    /// Tests mapping of non-generic Task types
    /// </summary>
    protected void Task_NonGeneric_Base()
    {
        // arrange
        var target = typeof(Task).ToContextualType();

        // act
        var modelRef = Map(target);

        // assert
        modelRef.As<PromiseRef>().Value.IsDefault();
        Models.IsEmpty();
    }
}

/// <summary>
/// Sample record for testing Task type mapping
/// </summary>
/// <param name="NullableTask">Task with nullable return type</param>
/// <param name="NotNullableTask">Task with non-nullable return type</param>
file record Sample(Task<string?> NullableTask, Task<string> NotNullableTask);
