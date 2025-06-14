using System;
using System.Collections.Generic;
using Annium.Core.DependencyInjection;
using Annium.Core.Runtime;
using Annium.Net.Types.Models;
using Annium.Net.Types.Refs;
using Annium.Net.Types.Tests.Lib.Mapper;
using Namotion.Reflection;

namespace Annium.Net.Types.Tests.Mapper;

/// <summary>
/// Concrete implementation of <see cref="ITestProvider"/> for type mapping tests
/// </summary>
internal class TestProvider : ITestProvider
{
    /// <summary>
    /// Gets the collection of models generated during mapping operations
    /// </summary>
    public IReadOnlyCollection<IModel> Models => _mapper.GetModels();

    /// <summary>
    /// The model mapper instance for type mapping operations
    /// </summary>
    private IModelMapper _mapper = default!;

    /// <summary>
    /// Configures the service container for testing
    /// </summary>
    /// <param name="container">The service container to configure</param>
    public void ConfigureContainer(IServiceContainer container)
    {
        container.AddRuntime(GetType().Assembly);
    }

    /// <summary>
    /// Sets up the test provider with the given service provider
    /// </summary>
    /// <param name="sp">The service provider to use for setup</param>
    public void Setup(IServiceProvider sp)
    {
        _mapper = sp.Resolve<IModelMapper>();
    }

    /// <summary>
    /// Maps a contextual type to a type reference
    /// </summary>
    /// <param name="type">The contextual type to map</param>
    /// <returns>The mapped type reference</returns>
    public IRef Map(ContextualType type) => _mapper.Map(type);
}
