using System.Text.Json;
using System.Text.Json.Serialization;
using Annium.Serialization.Json.Converters;
using Annium.Testing;
using Xunit;

namespace Annium.Serialization.Json.Tests;

/// <summary>
/// Tests for JsonSerializerOptionsExtensions utility methods
/// </summary>
public class JsonSerializerOptionsExtensionsTest
{
    /// <summary>
    /// Tests that InsertConverters inserts multiple converters at the specified index while
    /// preserving the caller's declared order and shifting any pre-existing converters down
    /// </summary>
    [Fact]
    public void InsertConverters_AtIndex_PreservesOrderAndShiftsExisting()
    {
        // arrange
        var opts = new JsonSerializerOptions();
        var existing = new ObjectArrayJsonConverterFactory();
        opts.Converters.Add(existing);

        var convA = new EnumJsonConverterFactory();
        var convB = new ConstructorJsonConverterFactory();
        var convC = new JsonNotIndentedJsonConverterFactory();

        // act
        opts.InsertConverters(0, convA, convB, convC);

        // assert
        opts.Converters[0].Is(convA);
        opts.Converters[1].Is(convB);
        opts.Converters[2].Is(convC);
        opts.Converters[3].Is(existing);
    }

    /// <summary>
    /// Tests that WithNumberHandling sets NumberHandling on the options and returns the same instance for chaining
    /// </summary>
    [Fact]
    public void WithNumberHandling_SetsHandling_ReturnsSameInstance()
    {
        // arrange
        var opts = new JsonSerializerOptions();

        // act
        var returned = opts.WithNumberHandling(JsonNumberHandling.AllowReadingFromString);

        // assert
        opts.NumberHandling.Is(JsonNumberHandling.AllowReadingFromString);
        returned.Is(opts);
    }

    /// <summary>
    /// Tests that UseCamelCaseNamingPolicy sets both PropertyNamingPolicy and DictionaryKeyPolicy to CamelCase
    /// </summary>
    [Fact]
    public void UseCamelCaseNamingPolicy_SetsBothPolicies_ToCamelCase()
    {
        // arrange
        var opts = new JsonSerializerOptions();

        // act
        opts.UseCamelCaseNamingPolicy();

        // assert
        opts.PropertyNamingPolicy.Is(JsonNamingPolicy.CamelCase);
        opts.DictionaryKeyPolicy.Is(JsonNamingPolicy.CamelCase);
    }

    /// <summary>
    /// Tests that UseDefaultNamingPolicy sets both PropertyNamingPolicy and DictionaryKeyPolicy to the same DefaultJsonNamingPolicy instance
    /// </summary>
    [Fact]
    public void UseDefaultNamingPolicy_SetsBothPolicies_ToDefaultNamingPolicy()
    {
        // arrange
        var opts = new JsonSerializerOptions();

        // act
        opts.UseDefaultNamingPolicy();

        // assert
        opts.PropertyNamingPolicy.IsNotNull();
        opts.DictionaryKeyPolicy.IsNotNull();
        opts.PropertyNamingPolicy.GetType().Name.Is("DefaultJsonNamingPolicy");
        opts.DictionaryKeyPolicy.GetType().Name.Is("DefaultJsonNamingPolicy");
        opts.PropertyNamingPolicy.Is(opts.DictionaryKeyPolicy);
    }
}
