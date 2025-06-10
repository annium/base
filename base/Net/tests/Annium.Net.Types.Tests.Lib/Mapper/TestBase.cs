using System.Collections.Generic;
using Annium.Net.Types.Models;
using Annium.Net.Types.Refs;
using Namotion.Reflection;
using Xunit;

namespace Annium.Net.Types.Tests.Lib.Mapper;

/// <summary>
/// Base class for all mapper test classes providing common test infrastructure
/// </summary>
public abstract class TestBase : Testing.TestBase
{
    /// <summary>
    /// Gets the collection of models generated during mapping operations
    /// </summary>
    protected IReadOnlyCollection<IModel> Models => _testProvider.Models;

    /// <summary>
    /// Gets the mapper configuration for test customization
    /// </summary>
    protected readonly IMapperConfig Config;

    /// <summary>
    /// The test provider instance for type mapping operations
    /// </summary>
    private readonly ITestProvider _testProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestBase"/> class
    /// </summary>
    /// <param name="testProvider">The test provider for type mapping operations</param>
    /// <param name="outputHelper">The test output helper</param>
    protected TestBase(ITestProvider testProvider, ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        _testProvider = testProvider;
        Register(container =>
        {
            container.AddModelMapper();
            testProvider.ConfigureContainer(container);
        });
        Setup(testProvider.Setup);
        Config = Get<IMapperConfig>();
    }

    /// <summary>
    /// Maps a contextual type to a type reference using the test provider
    /// </summary>
    /// <param name="type">The contextual type to map</param>
    /// <returns>The mapped type reference</returns>
    protected IRef Map(ContextualType type) => _testProvider.Map(type);
}
