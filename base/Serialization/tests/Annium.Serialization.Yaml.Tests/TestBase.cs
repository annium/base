using System;
using Annium.Core.DependencyInjection.Container;
using Annium.Core.Runtime;
using Annium.Serialization.Abstractions;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Annium.Serialization.Yaml.Tests;

/// <summary>
/// Base class for YAML serialization tests providing common setup and utilities
/// </summary>
public class TestBase
{
    /// <summary>
    /// Gets a configured YAML serializer for testing
    /// </summary>
    /// <param name="configure">Action to configure YAML serializer and deserializer builders</param>
    /// <returns>A YAML serializer with the specified configuration</returns>
    protected ISerializer<string> GetSerializer(Action<SerializerBuilder, DeserializerBuilder> configure)
    {
        var container = new ServiceContainer();
        container.AddRuntime(GetType().Assembly);
        container
            .AddSerializers()
            .WithYaml(
                (s, d) =>
                {
                    s.WithNamingConvention(CamelCaseNamingConvention.Instance);
                    d.WithNamingConvention(CamelCaseNamingConvention.Instance);
                    configure(s, d);
                }
            );

        var provider = container.BuildServiceProvider();

        return provider.ResolveSerializer<string>(Abstractions.Constants.DefaultKey, Constants.MediaType);
    }

    /// <summary>
    /// Gets a default YAML serializer for testing
    /// </summary>
    /// <returns>A YAML serializer with default configuration</returns>
    protected ISerializer<string> GetSerializer() => GetSerializer(delegate { });
}
