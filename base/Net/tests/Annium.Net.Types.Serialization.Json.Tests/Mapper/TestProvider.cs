using System;
using System.Collections.Generic;
using Annium.Core.DependencyInjection;
using Annium.Core.Runtime;
using Annium.Net.Types.Models;
using Annium.Net.Types.Refs;
using Annium.Net.Types.Tests.Lib.Mapper;
using Annium.Serialization.Abstractions;
using Annium.Serialization.Json;
using Namotion.Reflection;

namespace Annium.Net.Types.Serialization.Json.Tests.Mapper;

/// <summary>
/// Test provider implementation for JSON serialization testing
/// </summary>
internal class TestProvider : ITestProvider
{
    /// <summary>
    /// Gets the collection of models available for testing
    /// </summary>
    public IReadOnlyCollection<IModel> Models
    {
        get
        {
            var raw = _serializer.Serialize(_mapper.GetModels());
            var models = _serializer.Deserialize<IReadOnlyCollection<IModel>>(raw);
            return models;
        }
    }

    /// <summary>
    /// The model mapper instance
    /// </summary>
    private IModelMapper _mapper = default!;

    /// <summary>
    /// The serializer instance
    /// </summary>
    private ISerializer<string> _serializer = default!;

    /// <summary>
    /// Configures the service container with necessary services
    /// </summary>
    /// <param name="container">The service container to configure</param>
    public void ConfigureContainer(IServiceContainer container)
    {
        container.AddRuntime(GetType().Assembly);
        container
            .AddSerializers()
            .WithJson(
                opts =>
                {
                    opts.ConfigureForNetTypes();
                },
                isDefault: true
            );
    }

    /// <summary>
    /// Sets up the test provider with resolved services
    /// </summary>
    /// <param name="sp">The service provider</param>
    public void Setup(IServiceProvider sp)
    {
        _mapper = sp.Resolve<IModelMapper>();
        _serializer = sp.Resolve<ISerializer<string>>();
    }

    /// <summary>
    /// Maps a contextual type to a type reference
    /// </summary>
    /// <param name="type">The contextual type to map</param>
    /// <returns>The mapped type reference</returns>
    public IRef Map(ContextualType type)
    {
        var originalRef = _mapper.Map(type);
        var raw = _serializer.Serialize(originalRef);
        var modelRef = _serializer.Deserialize<IRef>(raw);

        return modelRef;
    }
}
