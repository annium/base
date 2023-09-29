using System.Collections.Generic;
using System.IO;
using System.Linq;
using Annium.Configuration.Abstractions;
using Annium.Configuration.Tests;
using Annium.Configuration.Tests.Lib;
using Annium.Core.DependencyInjection;
using Annium.Testing;
using Annium.Testing.Lib;
using Xunit;
using Xunit.Abstractions;
using YamlDotNet.Serialization;

namespace Annium.Configuration.Yaml.Tests;

public class YamlConfigurationProviderTest : TestBase
{
    public YamlConfigurationProviderTest(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        RegisterMapper();
    }

    [Fact]
    public void YamlConfiguration_Works()
    {
        // arrange
        var cfg = new Config
        {
            Flag = true,
            Plain = 7,
            Nullable = 3,
            Array = new[] { 4, 7 },
            Matrix = new List<int[]> { new[] { 3, 2 }, new[] { 5, 4 } },
            List = new List<Val> { new() { Plain = 8 }, new() { Array = new[] { 2m, 6m } } },
            Dictionary = new Dictionary<string, Val> { { "demo", new Val { Plain = 14, Array = new[] { 3m, 15m } } } },
            Nested = new Val { Plain = 4, Array = new[] { 4m, 13m } },
            Abstract = new ConfigTwo { Value = 10 },
        };

        string yamlFile = string.Empty;
        try
        {
            yamlFile = Path.GetTempFileName();
            var serializer = new SerializerBuilder().Build();
            File.WriteAllText(yamlFile, serializer.Serialize(cfg));
            Register(c => c.AddConfiguration<Config>(x => x.AddYamlFile(yamlFile)));

            // act
            var result = Get<Config>();
            var nested = Get<SomeConfig>();

            // assert
            result.IsNotDefault();
            result.Flag.IsTrue();
            result.Plain.Is(7);
            // result.Nullable.Is(3);
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
        finally
        {
            if (!string.IsNullOrWhiteSpace(yamlFile) && File.Exists(yamlFile))
                File.Delete(yamlFile);
        }
    }
}