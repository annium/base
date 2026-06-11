using System.Text.Json;
using System.Text.Json.Serialization;
using Annium.Core.DependencyInjection;
using Annium.Core.Runtime;
using Annium.Core.Runtime.Types;
using Annium.Logging.InMemory;
using Annium.Logging.Shared;
using Annium.Serialization.Abstractions;
using Annium.Testing;
using Xunit;

namespace Annium.Serialization.Json.Tests;

/// <summary>
/// Tests for JSON serialization configuration functionality
/// </summary>
public class ConfigurationTest
{
    /// <summary>
    /// Tests that multiple JSON serialization configurations can coexist and work correctly
    /// </summary>
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
        container
            .AddSerializers("a")
            .WithJson(x =>
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

    /// <summary>
    /// Tests that the no-key ResolveSerializer overload injects DefaultKey and resolves the registered serializer
    /// </summary>
    [Fact]
    public void ResolveSerializer_NoKeyOverload_UsesDefaultKey()
    {
        // arrange
        var container = new ServiceContainer();
        container.AddRuntime(GetType().Assembly);
        container.AddTime().WithRealTime().SetDefault();
        container.AddLogging();
        container.AddSerializers().WithJson(isDefault: true);
        var sp = container.BuildServiceProvider();
        sp.UseLogging(x => x.UseInMemory());

        var sample = new { Name = "test", Value = 42 };

        // act
        var serializer = sp.ResolveSerializer<string>(Constants.MediaType);
        var json = serializer.Serialize(sample);
        var result = serializer.Deserialize<SampleData>(json);

        // assert
        serializer.IsNotNull();
        result.Name.Is("test");
        result.Value.Is(42);
    }

    /// <summary>
    /// Simple data class for round-trip assertion
    /// </summary>
    private sealed class SampleData
    {
        /// <summary>
        /// Gets or sets the Name property
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Value property
        /// </summary>
        public int Value { get; set; }
    }

    /// <summary>
    /// Tests that WithJson(JsonSerializerOptions) registers a working serializer that honours
    /// the settings from the pre-configured options supplied by the caller
    /// </summary>
    [Fact]
    public void WithJsonOptions_PreConfiguredOptions_HonoursSuppliedOptions()
    {
        // arrange
        var typeManager = TypeManager.GetInstance(GetType().Assembly);
        var opts = new JsonSerializerOptions();
        opts.ConfigureDefault(typeManager);
        opts.UseCamelCaseNamingPolicy();

        var container = new ServiceContainer();
        container.AddRuntime(GetType().Assembly);
        container.AddTime().WithRealTime().SetDefault();
        container.AddLogging();
        container.AddSerializers().WithJson(opts, isDefault: true);
        var sp = container.BuildServiceProvider();
        sp.UseLogging(x => x.UseInMemory());

        var serializer = sp.ResolveSerializer<string>(Abstractions.Constants.DefaultKey, Constants.MediaType);
        var sample = new { Value = 1 };

        // act
        var result = serializer.Serialize(sample);

        // assert
        result.Is(@"{""value"":1}");
    }
}
