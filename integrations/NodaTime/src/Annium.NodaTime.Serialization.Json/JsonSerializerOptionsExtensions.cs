using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Annium.NodaTime.Serialization.Json;
using NodaTime;
using NodaTime.Xml;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class JsonSerializerOptionsExtensions
{
    public static JsonSerializerOptions ConfigureForNodaTime(
        this JsonSerializerOptions options
    )
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

    private static void AddDefaultConverters(
        IList<JsonConverter> converters,
        IDateTimeZoneProvider provider
    )
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

    private static void ReplaceExistingConverters<T>(IList<JsonConverter> converters, JsonConverter newConverter)
    {
        for (int i = converters.Count - 1; i >= 0; i--)
            if (converters[i].CanConvert(typeof(T)))
                converters.RemoveAt(i);

        converters.Add(newConverter);
    }
}