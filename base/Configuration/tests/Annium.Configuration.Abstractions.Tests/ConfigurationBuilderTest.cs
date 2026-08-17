using System.Collections.Generic;
using Annium.Configuration.Tests.Lib;
using Annium.Testing;
using Xunit;

namespace Annium.Configuration.Abstractions.Tests;

/// <summary>
/// Tests for configuration builder functionality.
/// </summary>
public class ConfigurationBuilderTest : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationBuilderTest"/> class.
    /// </summary>
    /// <param name="outputHelper">xUnit test output helper the test host logs through.</param>
    public ConfigurationBuilderTest(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        this.RegisterMapper();
        var cfg = new Dictionary<string[], string>();
        cfg[new[] { "plain" }] = "10";
        cfg[new[] { "abstract", "type" }] = "ConfigOne";
        cfg[new[] { "abstract", "value" }] = "14";
        cfg[new[] { "enum" }] = "two";
        Register((container, ct) => container.AddConfigurationAsync<Config>(x => x.Add(cfg), ct));
    }

    /// <summary>
    /// Tests that basic configuration building works correctly.
    /// </summary>
    [Fact]
    public void BaseBuilding_Works()
    {
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
