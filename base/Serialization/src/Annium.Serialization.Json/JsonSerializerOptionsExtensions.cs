using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Annium.Core.Runtime.Types;
using Annium.Reflection;
using Annium.Serialization.Json.Converters;
using Annium.Serialization.Json.Internal.Options;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

/// <summary>
/// Extension methods for JsonSerializerOptions providing convenient configuration and converter management.
/// </summary>
public static class JsonSerializerOptionsExtensions
{
    /// <summary>
    /// Properties that can be cloned when creating a copy of JsonSerializerOptions.
    /// </summary>
    private static readonly IReadOnlyCollection<PropertyInfo> _cloneableProperties = typeof(JsonSerializerOptions)
        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Where(x =>
            x.PropertyType.GetTargetImplementation(typeof(IEnumerable<>)) is null
            && x is { CanRead: true, CanWrite: true }
        )
        .ToArray();

    /// <summary>
    /// Configures the JsonSerializerOptions with default converters and settings for Annium serialization.
    /// </summary>
    /// <param name="options">The options to configure.</param>
    /// <param name="typeManager">The type manager for abstract type resolution.</param>
    /// <returns>The configured options.</returns>
    public static JsonSerializerOptions ConfigureDefault(this JsonSerializerOptions options, ITypeManager typeManager)
    {
        options.InsertConverter(0, new EnumJsonConverterFactory());
        options.InsertConverter(1, new JsonStringEnumConverter());
        options.InsertConverter(2, new MaterializableJsonConverterFactory());
        options.InsertConverter(3, new JsonNotIndentedJsonConverterFactory());
        options.InsertConverter(4, new ObjectArrayJsonConverterFactory());
        options.InsertConverter(5, new AbstractJsonConverterFactory(typeManager));
        options.InsertConverter(6, new ConstructorJsonConverterFactory());
        options.InsertConverter(7, new GenericDictionaryJsonConverterFactory());

        options.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
        options.UseDefaultNamingPolicy();
        options.IncludeFields = true;

        return options;
    }

    /// <summary>
    /// Removes all converters from the JsonSerializerOptions.
    /// </summary>
    /// <param name="options">The options to modify.</param>
    /// <returns>The modified options.</returns>
    public static JsonSerializerOptions ResetConverters(this JsonSerializerOptions options)
    {
        options.Converters.Clear();

        return options;
    }

    /// <summary>
    /// Sets the number handling policy for the JsonSerializerOptions.
    /// </summary>
    /// <param name="options">The options to modify.</param>
    /// <param name="numberHandling">The number handling policy to use.</param>
    /// <returns>The modified options.</returns>
    public static JsonSerializerOptions WithNumberHandling(
        this JsonSerializerOptions options,
        JsonNumberHandling numberHandling
    )
    {
        options.NumberHandling = numberHandling;

        return options;
    }

    /// <summary>
    /// Inserts a converter at the specified index in the converter collection.
    /// </summary>
    /// <typeparam name="TConverter">The type of converter to insert.</typeparam>
    /// <param name="options">The options to modify.</param>
    /// <param name="index">The index at which to insert the converter.</param>
    /// <returns>The modified options.</returns>
    public static JsonSerializerOptions InsertConverter<TConverter>(this JsonSerializerOptions options, int index)
        where TConverter : JsonConverter, new()
    {
        options.Converters.Insert(index, new TConverter());

        return options;
    }

    /// <summary>
    /// Inserts a converter at the specified index in the converter collection.
    /// </summary>
    /// <param name="options">The options to modify.</param>
    /// <param name="index">The index at which to insert the converter.</param>
    /// <param name="converter">The converter to insert.</param>
    /// <returns>The modified options.</returns>
    public static JsonSerializerOptions InsertConverter(
        this JsonSerializerOptions options,
        int index,
        JsonConverter converter
    )
    {
        options.Converters.Insert(index, converter);

        return options;
    }

    /// <summary>
    /// Inserts multiple converters at the specified index in the converter collection.
    /// </summary>
    /// <param name="options">The options to modify.</param>
    /// <param name="index">The index at which to insert the converters.</param>
    /// <param name="converters">The converters to insert.</param>
    /// <returns>The modified options.</returns>
    public static JsonSerializerOptions InsertConverters(
        this JsonSerializerOptions options,
        int index,
        params JsonConverter[] converters
    )
    {
        for (var i = converters.Length - 1; i >= 0; i--)
            options.Converters.Insert(index, converters[i]);

        return options;
    }

    /// <summary>
    /// Adds a converter to the end of the converter collection.
    /// </summary>
    /// <typeparam name="TConverter">The type of converter to add.</typeparam>
    /// <param name="options">The options to modify.</param>
    /// <returns>The modified options.</returns>
    public static JsonSerializerOptions AddConverter<TConverter>(this JsonSerializerOptions options)
        where TConverter : JsonConverter, new()
    {
        options.Converters.Add(new TConverter());

        return options;
    }

    /// <summary>
    /// Adds a converter to the end of the converter collection.
    /// </summary>
    /// <param name="options">The options to modify.</param>
    /// <param name="converter">The converter to add.</param>
    /// <returns>The modified options.</returns>
    public static JsonSerializerOptions AddConverter(this JsonSerializerOptions options, JsonConverter converter)
    {
        options.Converters.Add(converter);

        return options;
    }

    /// <summary>
    /// Adds multiple converters to the end of the converter collection.
    /// </summary>
    /// <param name="options">The options to modify.</param>
    /// <param name="converters">The converters to add.</param>
    /// <returns>The modified options.</returns>
    public static JsonSerializerOptions AddConverters(
        this JsonSerializerOptions options,
        params JsonConverter[] converters
    )
    {
        foreach (var converter in converters)
            options.Converters.Add(converter);

        return options;
    }

    /// <summary>
    /// Removes a converter of the specified type from the converter collection.
    /// </summary>
    /// <typeparam name="T">The type of converter to remove.</typeparam>
    /// <param name="opts">The options to modify.</param>
    /// <returns>The modified options.</returns>
    public static JsonSerializerOptions RemoveConverter<T>(this JsonSerializerOptions opts)
        where T : JsonConverter
    {
        var factory = opts.Converters.Single(x => x.GetType() == typeof(T));
        opts.Converters.Remove(factory);

        return opts;
    }

    /// <summary>
    /// Configures the options to use the default naming policy (no transformation).
    /// </summary>
    /// <param name="options">The options to modify.</param>
    /// <returns>The modified options.</returns>
    public static JsonSerializerOptions UseDefaultNamingPolicy(this JsonSerializerOptions options) =>
        options.UseNamingPolicy(new DefaultJsonNamingPolicy());

    /// <summary>
    /// Configures the options to use camelCase naming policy.
    /// </summary>
    /// <param name="options">The options to modify.</param>
    /// <returns>The modified options.</returns>
    public static JsonSerializerOptions UseCamelCaseNamingPolicy(this JsonSerializerOptions options) =>
        options.UseNamingPolicy(JsonNamingPolicy.CamelCase);

    /// <summary>
    /// Creates a deep copy of the JsonSerializerOptions.
    /// </summary>
    /// <param name="opts">The options to clone.</param>
    /// <returns>A new instance with the same configuration.</returns>
    public static JsonSerializerOptions Clone(this JsonSerializerOptions opts)
    {
        var clone = new JsonSerializerOptions();

        foreach (var converter in opts.Converters)
            clone.Converters.Add(converter);

        foreach (var p in _cloneableProperties)
            p.SetValue(clone, p.GetValue(opts));

        return clone;
    }

    /// <summary>
    /// Configures the options to write indented JSON.
    /// </summary>
    /// <param name="opts">The options to modify.</param>
    /// <returns>The modified options.</returns>
    public static JsonSerializerOptions Indented(this JsonSerializerOptions opts)
    {
        opts.WriteIndented = true;

        return opts;
    }

    /// <summary>
    /// Configures the options to write compact JSON without indentation.
    /// </summary>
    /// <param name="opts">The options to modify.</param>
    /// <returns>The modified options.</returns>
    public static JsonSerializerOptions NotIndented(this JsonSerializerOptions opts)
    {
        opts.WriteIndented = false;

        return opts;
    }

    /// <summary>
    /// Applies a naming policy to both property names and dictionary keys.
    /// </summary>
    /// <param name="options">The options to modify.</param>
    /// <param name="policy">The naming policy to apply.</param>
    /// <returns>The modified options.</returns>
    private static JsonSerializerOptions UseNamingPolicy(this JsonSerializerOptions options, JsonNamingPolicy policy)
    {
        options.DictionaryKeyPolicy = policy;
        options.PropertyNamingPolicy = policy;

        return options;
    }
}
