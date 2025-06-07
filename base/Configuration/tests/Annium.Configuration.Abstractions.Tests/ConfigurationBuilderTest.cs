using System.Collections.Generic;
using Annium.Configuration.Tests.Lib;
using Annium.Core.DependencyInjection;
using Annium.Testing;
using Xunit;

namespace Annium.Configuration.Abstractions.Tests;

/// <summary>
/// Tests for configuration builder functionality.
/// </summary>
public class ConfigurationBuilderTest : TestBase
{
    public ConfigurationBuilderTest(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        RegisterMapper();
    }

    /// <summary>
    /// Tests that basic configuration building works correctly.
    /// </summary>
    [Fact]
    public void BaseBuilding_Works()
    {
        // arrange
        var cfg = new Dictionary<string[], string>();
        cfg[new[] { "plain" }] = "10";
        cfg[new[] { "abstract", "type" }] = "ConfigOne";
        cfg[new[] { "abstract", "value" }] = "14";
        cfg[new[] { "enum" }] = "two";
        Register(container => container.AddConfiguration<Config>(x => x.Add(cfg)));

        // act
        var result = Get<Config>();
        var nested = Get<SomeConfig>();

        // assert
        result.IsNotDefault();
        result.Plain.Is(10);
        result.Abstract.IsEqual(nested);
        result.Enum.Is(SomeEnum.Two);
        nested.IsEqual(new ConfigOne { Value = 14 });
    }
}
