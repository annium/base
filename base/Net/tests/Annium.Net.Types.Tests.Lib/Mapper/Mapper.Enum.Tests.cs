using Annium.Net.Types.Extensions;
using Annium.Net.Types.Models;
using Annium.Net.Types.Refs;
using Annium.Testing;
using Annium.Testing.Collection;
using Namotion.Reflection;
using Xunit;

namespace Annium.Net.Types.Tests.Lib.Mapper;

/// <summary>
/// Base class for testing enum type mapping functionality
/// </summary>
public abstract class MapperEnumTestsBase : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MapperEnumTestsBase"/> class
    /// </summary>
    /// <param name="testProvider">The test provider for type mapping operations</param>
    /// <param name="outputHelper">The test output helper</param>
    protected MapperEnumTestsBase(ITestProvider testProvider, ITestOutputHelper outputHelper)
        : base(testProvider, outputHelper) { }

    /// <summary>
    /// Tests mapping of enum types
    /// </summary>
    protected void Enum_Base()
    {
        // arrange
        var target = typeof(Sample).ToContextualType();

        // act
        var modelRef = Map(target).As<EnumRef>();

        // assert
        modelRef.Namespace.Is(typeof(Sample).GetNamespace().ToString());
        modelRef.Name.Is(nameof(Sample));
        Models.Has(1);
        var model = Models.At(0).As<EnumModel>();
        model.Namespace.Is(typeof(Sample).GetNamespace());
        model.Name.Is(nameof(Sample));
        model.Values.Has(2);
        model.Values.At("A").Is(1);
        model.Values.At("B").Is(3);
    }
}

/// <summary>
/// Sample enum for testing enum type mapping
/// </summary>
file enum Sample
{
    A = 1,
    B = 3,
}
