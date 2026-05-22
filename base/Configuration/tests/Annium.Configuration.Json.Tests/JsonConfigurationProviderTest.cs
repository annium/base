using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Annium.Configuration.Abstractions;
using Annium.Configuration.Tests.Lib;
using Annium.Core.DependencyInjection;
using Annium.Core.Runtime;
using Annium.Logging.Shared;
using Annium.Serialization.Abstractions;
using Annium.Serialization.Json;
using Annium.Testing;
using Xunit;

namespace Annium.Configuration.Json.Tests;

/// <summary>
/// Tests for JSON configuration provider functionality.
/// </summary>
public class JsonConfigurationProviderTest : TestBase
{
    private readonly string _jsonFile;

    public JsonConfigurationProviderTest(ITestOutputHelper outputHelper)
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
            Tuple = ("demo|", 11),
        };

        _jsonFile = Path.GetTempFileName();
        var container = new ServiceContainer();
        container.AddRuntime(GetType().Assembly);
        container.AddTime().WithRealTime().SetDefault();
        container.AddSerializers().WithJson(isDefault: true);
        container.AddLogging();
        var serializer = container.BuildServiceProvider().Resolve<ISerializer<string>>();
        File.WriteAllText(_jsonFile, serializer.Serialize(cfg));

        Register((c, ct) => c.AddConfigurationAsync<Config>(x => x.AddJsonFile(_jsonFile), ct));
    }

    /// <inheritdoc/>
    public override async ValueTask DisposeAsync()
    {
        if (!string.IsNullOrWhiteSpace(_jsonFile) && File.Exists(_jsonFile))
            File.Delete(_jsonFile);
        await base.DisposeAsync();
    }

    /// <summary>
    /// Tests that JSON configuration works correctly.
    /// </summary>
    [Fact]
    public void JsonConfiguration_Works()
    {
        // act
        var result = Get<Config>();
        var nested = Get<SomeConfig>();

        // assert
        result.IsNotDefault();
        result.Flag.IsTrue();
        result.Plain.Is(7);
        // result.Nullable.Is(3);
        Enumerable.SequenceEqual(result.Array, new[] { 4, 7 }).IsTrue();
        result.Matrix.Has(2);
        Enumerable.SequenceEqual(result.Matrix.At(0), new[] { 3, 2 }).IsTrue();
        Enumerable.SequenceEqual(result.Matrix.At(1), new[] { 5, 4 }).IsTrue();
        result.List.Has(2);
        result.List[0].Plain.Is(8);
        result.List[0].Array.IsEmpty();
        result.List[1].Plain.Is(0);
        Enumerable.SequenceEqual(result.List[1].Array, new[] { 2m, 6m }).IsTrue();
        IDictionary<string, Val> dict = result.Dictionary;
        dict.Has(1);
        dict.At("demo").Plain.Is(14);
        Enumerable.SequenceEqual(dict.At("demo").Array, new[] { 3m, 15m }).IsTrue();
        result.Nested.Plain.Is(4);
        Enumerable.SequenceEqual(result.Nested.Array, new[] { 4m, 13m }).IsTrue();
        result.Abstract.As<ConfigTwo>().Value.Is(10);
        result.Abstract.IsEqual(nested);
        nested.Is(new ConfigTwo { Value = 10 });
    }

    /// <summary>
    /// Malformed JSON content surfaces as a <see cref="System.Text.Json.JsonException"/> from
    /// the provider; via the build pipeline it lands wrapped in <see cref="System.AggregateException"/>.
    /// </summary>
    [Fact]
    public async Task Read_MalformedJson_ThrowsJsonException()
    {
        var bad = Path.GetTempFileName();
        try
        {
            File.WriteAllText(bad, "{bad json");
            var container = ConfigurationFactory.CreateContainer();
            container.AddJsonFile(bad, optional: false);

            var ex = await Wrap.It(async () => await container.BuildAsync(TestContext.Current.CancellationToken))
                .ThrowsAsync<AggregateException>();
            ex.InnerExceptions.Has(1);
            ex.InnerExceptions[0].As<JsonException>();
        }
        finally
        {
            File.Delete(bad);
        }
    }
}
