using System.Text.Json.Serialization;
using Annium.Core.DependencyInjection;
using Annium.Serialization.Abstractions;
using Annium.Testing;
using Xunit;

namespace Annium.Serialization.Json.Tests;

public class ConfigurationTest
{
    [Fact]
    public void MultipleConfigurations_Work()
    {
        // arrange
        var container = new ServiceContainer();
        container.AddRuntime(GetType().Assembly);
        container.AddTime().WithRealTime().SetDefault();
        container.AddLogging();
        // default
        container.AddSerializers().WithJson(isDefault: true);
        // custom
        container.AddSerializers("a").WithJson(x =>
        {
            x.UseCamelCaseNamingPolicy();
            x.NumberHandling = JsonNumberHandling.WriteAsString;
        });
        container.AddSerializers("b").WithJson(x => x.UseCamelCaseNamingPolicy());
        var sp = container.BuildServiceProvider();
        sp.UseLogging(x => x.UseInMemory());

        var serializerDefault = sp.ResolveSerializer<string>(Abstractions.Constants.DefaultKey, Constants.MediaType);
        var serializerA = sp.ResolveSerializer<string>("a", Constants.MediaType);
        var serializerB = sp.ResolveSerializer<string>("b", Constants.MediaType);
        var sample = new { X = 1 };

        // act
        var resultDefault = serializerDefault.Serialize(sample);
        var resultA = serializerA.Serialize(sample);
        var resultB = serializerB.Serialize(sample);

        // assert
        sp.Resolve<ISerializer<string>>().Is(serializerDefault);
        resultDefault.Is(@"{""X"":1}");
        resultA.Is(@"{""x"":""1""}");
        resultB.Is(@"{""x"":1}");
    }
}