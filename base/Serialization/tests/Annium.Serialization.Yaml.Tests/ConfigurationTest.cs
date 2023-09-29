using System;
using Annium.Core.DependencyInjection;
using Annium.Serialization.Abstractions;
using Annium.Testing;
using Xunit;
using YamlDotNet.Serialization.NamingConventions;

namespace Annium.Serialization.Yaml.Tests;

public class ConfigurationTest
{
    [Fact]
    public void MultipleConfigurations_Work()
    {
        // arrange
        var container = new ServiceContainer();
        container.AddRuntime(GetType().Assembly);
        // default
        container.AddSerializers().WithYaml(isDefault: true);
        // custom
        container.AddSerializers("a").WithYaml((s, d) =>
        {
            s.WithNamingConvention(CamelCaseNamingConvention.Instance);
            d.WithNamingConvention(CamelCaseNamingConvention.Instance);
        });
        var sp = container.BuildServiceProvider();

        var serializerDefault = sp.ResolveSerializer<string>(Abstractions.Constants.DefaultKey, Constants.MediaType);
        var serializerA = sp.ResolveSerializer<string>("a", Constants.MediaType);
        var sample = new { X = "a b", Y = new[] { 1, 2 } };

        // act
        var resultDefault = serializerDefault.Serialize(sample);
        var resultA = serializerA.Serialize(sample);

        // assert
        sp.Resolve<ISerializer<string>>().Is(serializerDefault);
        var n = Environment.NewLine;
        resultDefault.Is($"X: a b{n}Y:{n}- 1{n}- 2{n}");
        resultA.Is($"x: a b{n}y:{n}- 1{n}- 2{n}");
    }
}