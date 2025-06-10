using Annium.Net.Types.Extensions;
using Annium.Net.Types.Models;
using Annium.Net.Types.Refs;
using Annium.Testing;
using Annium.Testing.Collection;
using Namotion.Reflection;
using Xunit;

namespace Annium.Net.Types.Tests.Lib.Mapper;

/// <summary>
/// Base class for testing included type mapping functionality
/// </summary>
public abstract class MapperIncludedTestsBase : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MapperIncludedTestsBase"/> class
    /// </summary>
    /// <param name="testProvider">The test provider for type mapping operations</param>
    /// <param name="outputHelper">The test output helper</param>
    protected MapperIncludedTestsBase(ITestProvider testProvider, ITestOutputHelper outputHelper)
        : base(testProvider, outputHelper) { }

    /// <summary>
    /// Tests mapping with explicitly included types
    /// </summary>
    protected void Included_Base()
    {
        // arrange
        Config.Include(typeof(Sample));
        var target = typeof(int).ToContextualType();

        // act
        var modelRef = Map(target).As<BaseTypeRef>();

        // assert
        modelRef.Name.Is(BaseType.Int);
        Models.Has(1);
        var sample = Models.At(0).As<StructModel>();
        sample.Namespace.Is(typeof(Sample).Namespace!.ToNamespace());
        sample.Name.Is(nameof(Sample));
    }
}

/// <summary>
/// Sample record for testing included type mapping
/// </summary>
file record Sample;
