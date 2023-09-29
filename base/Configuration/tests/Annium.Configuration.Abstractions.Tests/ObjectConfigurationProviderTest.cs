using System.Collections.Generic;
using System.Linq;
using Annium.Configuration.Tests;
using Annium.Configuration.Tests.Lib;
using Annium.Core.DependencyInjection;
using Annium.Testing;
using Annium.Testing.Lib;
using Xunit;
using Xunit.Abstractions;

namespace Annium.Configuration.Abstractions.Tests;

public class ObjectConfigurationProviderTest : TestBase
{
    public ObjectConfigurationProviderTest(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        RegisterMapper();
    }

    [Fact]
    public void Works()
    {
        // arrange
        var cfg = new Config
        {
            Flag = true,
            Plain = 7,
            Array = new[] { 4, 7 },
            Matrix = new List<int[]> { new[] { 3, 2 }, new[] { 5, 4 } },
            List = new List<Val> { new() { Plain = 8 }, new() { Array = new[] { 2m, 6m } } },
            Dictionary = new Dictionary<string, Val> { { "demo", new Val { Plain = 14, Array = new[] { 3m, 15m } } } },
            Nested = new Val { Plain = 4, Array = new[] { 4m, 13m } },
            Abstract = new ConfigTwo { Value = 10 },
            Tuple = ("demo|", 11),
        };
        Register(container => container.AddConfiguration<Config>(x => x.Add(cfg)));

        // act
        var result = Get<Config>();
        var nested = Get<SomeConfig>();

        // assert
        result.IsNotDefault();
        result.Flag.IsTrue();
        result.Plain.Is(7);
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
        nested.IsEqual(new ConfigTwo { Value = 10 });
    }
}