using System;
using System.Collections.Generic;
using Annium.Core.DependencyInjection;
using Annium.Net.Types.Models;
using Annium.Net.Types.Refs;
using Namotion.Reflection;

namespace Annium.Net.Types.Tests.Lib.Mapper;

/// <summary>
/// Provides test functionality for type mapping operations
/// </summary>
public interface ITestProvider
{
    /// <summary>
    /// Gets the collection of models generated during mapping
    /// </summary>
    IReadOnlyCollection<IModel> Models { get; }

    /// <summary>
    /// Configures the service container for testing
    /// </summary>
    /// <param name="container">The service container to configure</param>
    void ConfigureContainer(IServiceContainer container);

    /// <summary>
    /// Sets up the test provider with the given service provider
    /// </summary>
    /// <param name="sp">The service provider to use for setup</param>
    void Setup(IServiceProvider sp);

    /// <summary>
    /// Maps a contextual type to a type reference
    /// </summary>
    /// <param name="type">The contextual type to map</param>
    /// <returns>The mapped type reference</returns>
    IRef Map(ContextualType type);
}
