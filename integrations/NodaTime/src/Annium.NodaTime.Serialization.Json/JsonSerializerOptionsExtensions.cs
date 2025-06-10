using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using NodaTime;
using NodaTime.Xml;

namespace Annium.NodaTime.Serialization.Json;

/// <summary>
/// Extension methods for configuring JsonSerializerOptions with NodaTime converters.
/// </summary>
public static class JsonSerializerOptionsExtensions
{
    /// <summary>
    /// Configures the JsonSerializerOptions with default NodaTime converters and appropriate settings.
    /// </summary>
    /// <param name="options">The JsonSerializerOptions to configure.</param>
    /// <returns>The configured JsonSerializerOptions for method chaining.</returns>
    public static JsonSerializerOptions ConfigureForNodaTime(this JsonSerializerOptions options)
    {
        options.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
        options.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;

        AddDefaultConverters(options.Converters, XmlSerializationSettings.DateTimeZoneProvider);

        return options;
    }

    /// <summary>
    /// Configures the given serializer settings to use <see cref="Converters.IsoIntervalConverter"/>.
    /// Any other converters which can convert <see cref="Interval"/> are removed from the serializer.
    /// </summary>
    /// <param name="options">The existing serializer settings to add Noda Time converters to.</param>
    /// <returns>The original <paramref name="options"/> value, for further chaining.</returns>
    public static JsonSerializerOptions WithIsoIntervalConverter(this JsonSerializerOptions options)
    {
        ReplaceExistingConverters<Interval>(options.Converters, Converters.IsoIntervalConverter);

        return options;
    }

    /// <summary>
    /// Configures the given serializer settings to use <see cref="Converters.IsoDateIntervalConverter"/>.
    /// Any other converters which can convert <see cref="DateInterval"/> are removed from the serializer.
    /// </summary>
    /// <param name="options">The existing serializer settings to add Noda Time converters to.</param>
    /// <returns>The original <paramref name="options"/> value, for further chaining.</returns>
    public static JsonSerializerOptions WithIsoDateIntervalConverter(this JsonSerializerOptions options)
    {
        ReplaceExistingConverters<DateInterval>(options.Converters, Converters.IsoDateIntervalConverter);

        return options;
    }

    /// <summary>
    /// Adds all default NodaTime converters to the specified converter collection.
    /// </summary>
    /// <param name="converters">The converter collection to add to.</param>
    /// <param name="provider">The date time zone provider to use for zone-aware converters.</param>
    private static void AddDefaultConverters(IList<JsonConverter> converters, IDateTimeZoneProvider provider)
    {
        converters.Insert(0, Converters.InstantConverter);
        converters.Insert(0, Converters.IntervalConverter);
        converters.Insert(0, Converters.LocalDateConverter);
        converters.Insert(0, Converters.LocalDateTimeConverter);
        converters.Insert(0, Converters.LocalTimeConverter);
        converters.Insert(0, Converters.DateIntervalConverter);
        converters.Insert(0, Converters.OffsetConverter);
        converters.Insert(0, Converters.CreateDateTimeZoneConverter(provider));
        converters.Insert(0, Converters.DurationConverter);
        converters.Insert(0, Converters.RoundtripPeriodConverter);
        converters.Insert(0, Converters.OffsetDateTimeConverter);
        converters.Insert(0, Converters.OffsetDateConverter);
        converters.Insert(0, Converters.OffsetTimeConverter);
        converters.Insert(0, Converters.YearMonthConverter);
        converters.Insert(0, Converters.CreateZonedDateTimeConverter(provider));
    }

    /// <summary>
    /// Replaces any existing converters for the specified type with the new converter.
    /// </summary>
    /// <typeparam name="T">The type whose converters should be replaced.</typeparam>
    /// <param name="converters">The converter collection to modify.</param>
    /// <param name="newConverter">The new converter to add.</param>
    private static void ReplaceExistingConverters<T>(IList<JsonConverter> converters, JsonConverter newConverter)
    {
        for (var i = converters.Count - 1; i >= 0; i--)
            if (converters[i].CanConvert(typeof(T)))
                converters.RemoveAt(i);

        converters.Add(newConverter);
    }
}
