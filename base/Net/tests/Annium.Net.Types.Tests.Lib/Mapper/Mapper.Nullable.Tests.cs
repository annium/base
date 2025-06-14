using Annium.Net.Types.Refs;
using Annium.Testing;
using Namotion.Reflection;
using Xunit;

namespace Annium.Net.Types.Tests.Lib.Mapper;

/// <summary>
/// Base class for testing nullable type mapping functionality
/// </summary>
public abstract class MapperNullableTestsBase : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MapperNullableTestsBase"/> class
    /// </summary>
    /// <param name="testProvider">The test provider for type mapping operations</param>
    /// <param name="outputHelper">The test output helper</param>
    protected MapperNullableTestsBase(ITestProvider testProvider, ITestOutputHelper outputHelper)
        : base(testProvider, outputHelper) { }

    /// <summary>
    /// Tests mapping of nullable value types (structs)
    /// </summary>
    protected void Nullable_BaseType_Struct_Base()
    {
        // arrange
        var target = typeof(int?).ToContextualType();

        // act
        var model = Map(target);

        // assert
        model.As<NullableRef>().Value.As<BaseTypeRef>().Name.Is(BaseType.Int);
        Models.IsEmpty();
    }

    /// <summary>
    /// Tests mapping of nullable reference types (classes)
    /// </summary>
    protected void Nullable_BaseType_Class_Base()
    {
        // arrange
        var target = typeof(Sample).ToContextualType().GetProperty(nameof(Sample.Value))!.AccessorType;

        // act
        var model = Map(target);

        // assert
        model.As<NullableRef>().Value.As<BaseTypeRef>().Name.Is(BaseType.String);
        Models.IsEmpty();
    }
}

/// <summary>
/// Sample record for testing nullable type mapping
/// </summary>
/// <param name="Value">The nullable string value</param>
file record Sample(string? Value);
