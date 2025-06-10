using System.Collections.Generic;
using Annium.Net.Types.Refs;
using Annium.Testing;
using Annium.Testing.Collection;
using Namotion.Reflection;
using Xunit;

namespace Annium.Net.Types.Tests.Lib.Mapper;

/// <summary>
/// Base class for testing generic parameter type mapping functionality
/// </summary>
public abstract class MapperGenericParameterTestsBase : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MapperGenericParameterTestsBase"/> class
    /// </summary>
    /// <param name="testProvider">The test provider for type mapping operations</param>
    /// <param name="outputHelper">The test output helper</param>
    protected MapperGenericParameterTestsBase(ITestProvider testProvider, ITestOutputHelper outputHelper)
        : base(testProvider, outputHelper) { }

    /// <summary>
    /// Tests mapping of non-nullable generic parameters
    /// </summary>
    protected void GenericParameter_NotNullable_Base()
    {
        // arrange
        var target = typeof(List<>).GetGenericArguments()[0].ToContextualType();

        // act
        var modelRef = Map(target);

        // assert
        modelRef.As<GenericParameterRef>().Name.Is(target.Type.Name);
        Models.IsEmpty();
    }

    /// <summary>
    /// Tests mapping of nullable generic parameters
    /// </summary>
    protected void GenericParameter_Nullable_Base()
    {
        // arrange
        var target = typeof(Sample<>).ToContextualType().GetProperty(nameof(Sample<string>.Value))!.AccessorType;

        // act
        var modelRef = Map(target);

        // assert
        modelRef.As<NullableRef>().Value.As<GenericParameterRef>().Name.Is(target.Type.Name);
        Models.IsEmpty();
    }
}

/// <summary>
/// Sample generic record for testing generic parameter mapping
/// </summary>
/// <typeparam name="T">The generic type parameter</typeparam>
/// <param name="Value">The nullable value</param>
file record Sample<T>(T? Value)
    where T : notnull;
