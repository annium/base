using Annium.Net.Types.Refs;
using Annium.Testing;
using Namotion.Reflection;
using Xunit;

namespace Annium.Net.Types.Tests.Lib.Mapper;

/// <summary>
/// Base class for testing excluded type mapping functionality
/// </summary>
public abstract class MapperExcludedTestsBase : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MapperExcludedTestsBase"/> class
    /// </summary>
    /// <param name="testProvider">The test provider for type mapping operations</param>
    /// <param name="outputHelper">The test output helper</param>
    protected MapperExcludedTestsBase(ITestProvider testProvider, ITestOutputHelper outputHelper)
        : base(testProvider, outputHelper) { }

    /// <summary>
    /// Tests mapping of types that are excluded from detailed processing
    /// </summary>
    protected void Excluded_Base()
    {
        // arrange
        Config.Exclude(Match.Is(typeof(Sample)));
        var target = typeof(Sample).ToContextualType();

        // act
        var modelRef = Map(target).As<StructRef>();

        // assert
        modelRef.Namespace.Is(typeof(Sample).Namespace);
        modelRef.Name.Is(nameof(Sample));
        modelRef.Args.IsEmpty();
        Models.IsEmpty();
    }
}

/// <summary>
/// Sample record for testing excluded type mapping
/// </summary>
file record Sample;
