using System;
using Annium.Core.DependencyInjection;
using Annium.Serialization.Abstractions;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Annium.Serialization.Yaml.Tests;

public class TestBase
{
    protected ISerializer<string> GetSerializer(Action<SerializerBuilder, DeserializerBuilder> configure)
    {
        var container = new ServiceContainer();
        container.AddRuntime(GetType().Assembly);
        container.AddSerializers()
            .WithYaml((s, d) =>
            {
                s.WithNamingConvention(CamelCaseNamingConvention.Instance);
                d.WithNamingConvention(CamelCaseNamingConvention.Instance);
                configure(s, d);
            });

        var provider = container.BuildServiceProvider();

        return provider.ResolveSerializer<string>(Abstractions.Constants.DefaultKey, Constants.MediaType);
    }

    protected ISerializer<string> GetSerializer() => GetSerializer(delegate { });
}