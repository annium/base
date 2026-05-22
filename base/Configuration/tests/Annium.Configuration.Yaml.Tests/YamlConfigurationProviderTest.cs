using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Annium.Configuration.Abstractions;
using Annium.Configuration.Tests.Lib;
using Annium.Testing;
using Xunit;
using YamlDotNet.Serialization;

namespace Annium.Configuration.Yaml.Tests;

/// <summary>
/// Tests for YAML configuration provider functionality.
/// </summary>
public class YamlConfigurationProviderTest : TestBase
{
    private readonly string _yamlFile;

    public YamlConfigurationProviderTest(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        this.RegisterMapper();

        var cfg = new Config
        {
            Flag = true,
            Plain = 7,
            Nullable = 3,
            Array = new[] { 4, 7 },
            Matrix = new List<int[]> { new[] { 3, 2 }, new[] { 5, 4 } },
            List = new List<Val>
            {
                new() { Plain = 8 },
                new() { Array = new[] { 2m, 6m } },
            },
            Dictionary = new Dictionary<string, Val>
            {
                {
                    "demo",
                    new Val { Plain = 14, Array = new[] { 3m, 15m } }
                },
            },
            Nested = new Val { Plain = 4, Array = new[] { 4m, 13m } },
            Abstract = new ConfigTwo { Value = 10 },
        };

        _yamlFile = Path.GetTempFileName();
        var serializer = new SerializerBuilder().Build();
        File.WriteAllText(_yamlFile, serializer.Serialize(cfg));

        Register((c, ct) => c.AddConfigurationAsync<Config>(x => x.AddYamlFile(_yamlFile), ct));
    }

    /// <inheritdoc/>
    public override async ValueTask DisposeAsync()
    {
        if (!string.IsNullOrWhiteSpace(_yamlFile) && File.Exists(_yamlFile))
            File.Delete(_yamlFile);
        await base.DisposeAsync();
    }

    /// <summary>
    /// Tests that YAML configuration works correctly.
    /// </summary>
    [Fact]
    public void YamlConfiguration_Works()
    {
        // act
        var result = Get<Config>();
        var nested = Get<SomeConfig>();

        // assert
        result.IsNotDefault();
        result.Flag.IsTrue();
        result.Plain.Is(7);
        result.Nullable.IsNotDefault();
        result.Nullable.Value.Is(3m);
        result.Array.SequenceEqual(new[] { 4, 7 }).IsTrue();
        result.Matrix.Has(2);
        result.Matrix.At(0).SequenceEqual(new[] { 3, 2 }).IsTrue();
        result.Matrix.At(1).SequenceEqual(new[] { 5, 4 }).IsTrue();
        result.List.Has(2);
        result.List[0].Plain.Is(8);
        result.List[0].Array.IsEmpty();
        result.List[1].Plain.Is(0);
        result.List[1].Array.SequenceEqual(new[] { 2m, 6m }).IsTrue();
        IDictionary<string, Val> dict = result.Dictionary;
        dict.Has(1);
        dict.At("demo").Plain.Is(14);
        dict.At("demo").Array.SequenceEqual(new[] { 3m, 15m }).IsTrue();
        result.Nested.Plain.Is(4);
        result.Nested.Array.SequenceEqual(new[] { 4m, 13m }).IsTrue();
        result.Abstract.As<ConfigTwo>().Value.Is(10);
        result.Abstract.IsEqual(nested);
        nested.Is(new ConfigTwo { Value = 10 });
    }

    /// <summary>
    /// An empty YAML document (no top-level node) yields an empty configuration container.
    /// </summary>
    [Fact]
    public async Task Read_EmptyYamlDocument_ReturnsEmptyData()
    {
        var path = Path.GetTempFileName();
        try
        {
            File.WriteAllText(path, string.Empty);
            var container = ConfigurationFactory.CreateContainer();
            container.AddYamlFile(path, optional: false);

            await container.BuildAsync(TestContext.Current.CancellationToken);

            container.Get().Count.Is(0);
        }
        finally
        {
            File.Delete(path);
        }
    }

    /// <summary>
    /// A YAML document whose root is a scalar (e.g. <c>42</c>) is invalid for configuration —
    /// the provider throws <see cref="InvalidOperationException"/> wrapped in <see cref="AggregateException"/>.
    /// </summary>
    [Fact]
    public async Task Read_NonMappingRoot_ThrowsInvalidOperationException()
    {
        var path = Path.GetTempFileName();
        try
        {
            File.WriteAllText(path, "42");
            var container = ConfigurationFactory.CreateContainer();
            container.AddYamlFile(path, optional: false);

            var ex = await Wrap.It(async () => await container.BuildAsync(TestContext.Current.CancellationToken))
                .ThrowsAsync<AggregateException>();
            ex.InnerExceptions.Has(1);
            ex.InnerExceptions[0].As<InvalidOperationException>();
        }
        finally
        {
            File.Delete(path);
        }
    }

    /// <summary>
    /// A YAML document where a mapping key is itself a mapping node (complex key syntax,
    /// <c>? { a: 1 } : value</c>) triggers the non-scalar-key guard, surfacing
    /// <see cref="InvalidOperationException"/>.
    /// </summary>
    [Fact]
    public async Task Read_NonScalarMappingKey_ThrowsInvalidOperationException()
    {
        var path = Path.GetTempFileName();
        try
        {
            // YAML 1.2 explicit-key form: "? mapping" makes the key itself a mapping node.
            File.WriteAllText(path, "? { a: 1 }\n: value\n");
            var container = ConfigurationFactory.CreateContainer();
            container.AddYamlFile(path, optional: false);

            var ex = await Wrap.It(async () => await container.BuildAsync(TestContext.Current.CancellationToken))
                .ThrowsAsync<AggregateException>();
            ex.InnerExceptions.Has(1);
            var inner = ex.InnerExceptions[0].As<InvalidOperationException>();
            inner.Message.Contains("scalar").IsTrue($"expected message mentioning 'scalar'; got: {inner.Message}");
        }
        finally
        {
            File.Delete(path);
        }
    }
}
