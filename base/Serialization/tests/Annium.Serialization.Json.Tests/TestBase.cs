using System;
using System.Text.Json;
using Annium.Core.DependencyInjection.Container;
using Annium.Core.Runtime;
using Annium.Logging.InMemory;
using Annium.Logging.Shared;
using Annium.Serialization.Abstractions;

namespace Annium.Serialization.Json.Tests;

/// <summary>
/// Base class for JSON serialization tests providing common setup and utilities
/// </summary>
public class TestBase
{
    /// <summary>
    /// Gets a configured JSON serializer for testing
    /// </summary>
    /// <param name="configure">Action to configure JSON serializer options</param>
    /// <returns>A JSON serializer with the specified configuration</returns>
    protected ISerializer<string> GetSerializer(Action<JsonSerializerOptions> configure)
    {
        var container = new ServiceContainer();
        container.AddRuntime(GetType().Assembly);
        container.AddTime().WithRealTime().SetDefault();
        container
            .AddSerializers()
            .WithJson(opts =>
            {
                opts.UseCamelCaseNamingPolicy();
                configure(opts);
            });
        container.AddLogging();

        var provider = container.BuildServiceProvider();

        provider.UseLogging(x => x.UseInMemory());

        return provider.ResolveSerializer<string>(Abstractions.Constants.DefaultKey, Constants.MediaType);
    }

    /// <summary>
    /// Gets a default JSON serializer for testing
    /// </summary>
    /// <returns>A JSON serializer with default configuration</returns>
    protected ISerializer<string> GetSerializer() => GetSerializer(delegate { });
}
