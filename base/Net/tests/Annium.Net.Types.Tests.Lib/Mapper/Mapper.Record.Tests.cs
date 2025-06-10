using System.Collections.Generic;
using System.Collections.Immutable;
using Annium.Net.Types.Refs;
using Annium.Testing;
using Annium.Testing.Collection;
using Namotion.Reflection;
using Xunit;

namespace Annium.Net.Types.Tests.Lib.Mapper;

/// <summary>
/// Base class for testing record type mapping functionality
/// </summary>
public abstract class MapperRecordTestsBase : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MapperRecordTestsBase"/> class
    /// </summary>
    /// <param name="testProvider">The test provider for type mapping operations</param>
    /// <param name="outputHelper">The test output helper</param>
    protected MapperRecordTestsBase(ITestProvider testProvider, ITestOutputHelper outputHelper)
        : base(testProvider, outputHelper) { }

    /// <summary>
    /// Tests mapping of interface-based record types with generic parameters
    /// </summary>
    protected void Interface_Base()
    {
        // arrange
        var target = typeof(IDictionary<,>).ToContextualType();

        // act
        var modelRef = Map(target);

        // assert
        modelRef.As<RecordRef>().Key.As<GenericParameterRef>().Name.Is("TKey");
        modelRef.As<RecordRef>().Value.As<GenericParameterRef>().Name.Is("TValue");
        Models.IsEmpty();
    }

    /// <summary>
    /// Tests mapping of concrete implementation record types
    /// </summary>
    protected void Implementation_Base()
    {
        // arrange
        var target = typeof(ImmutableDictionary<string, int>).ToContextualType();

        // act
        var modelRef = Map(target);

        // assert
        modelRef.As<RecordRef>().Key.As<BaseTypeRef>().Name.Is(BaseType.String);
        modelRef.As<RecordRef>().Value.As<BaseTypeRef>().Name.Is(BaseType.Int);
        Models.IsEmpty();
    }
}
