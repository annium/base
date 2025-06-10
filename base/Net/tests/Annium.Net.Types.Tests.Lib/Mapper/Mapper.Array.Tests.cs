using System.Collections.Generic;
using Annium.Net.Types.Refs;
using Annium.Testing;
using Annium.Testing.Collection;
using Namotion.Reflection;
using Xunit;

namespace Annium.Net.Types.Tests.Lib.Mapper;

/// <summary>
/// Base class for testing array type mapping functionality
/// </summary>
public abstract class MapperArrayTestsBase : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MapperArrayTestsBase"/> class
    /// </summary>
    /// <param name="testProvider">The test provider for type mapping operations</param>
    /// <param name="outputHelper">The test output helper</param>
    protected MapperArrayTestsBase(ITestProvider testProvider, ITestOutputHelper outputHelper)
        : base(testProvider, outputHelper) { }

    /// <summary>
    /// Tests mapping of base type arrays
    /// </summary>
    protected void Array_Base()
    {
        // arrange
        var target = typeof(int[]).ToContextualType();

        // act
        var modelRef = Map(target);

        // assert
        modelRef.As<ArrayRef>().Value.As<BaseTypeRef>().Name.Is(BaseType.Int);
        Models.IsEmpty();
    }

    /// <summary>
    /// Tests mapping of array-like generic types
    /// </summary>
    protected void ArrayLike_Base()
    {
        // arrange
        var target = typeof(HashSet<>).ToContextualType();

        // act
        var modelRef = Map(target);

        // assert
        modelRef.As<ArrayRef>().Value.As<GenericParameterRef>().Name.Is("T");
        Models.IsEmpty();
    }
}
