using System.Text.Json;
using Annium.Testing;
using NodaTime;
using NodaTime.Utility;
using Xunit;
using static Annium.NodaTime.Serialization.Json.Tests.TestHelper;

namespace Annium.NodaTime.Serialization.Json.Tests;

/// <summary>
/// Tests for error/throw branches across ConverterBase and the specialized converters.
/// </summary>
public class ConverterErrorBranchesTest
{
    // -------------------------------------------------------------------------
    // ConverterBase<T> — null token
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that a JSON null token deserializing into a non-nullable Instant throws InvalidNodaDataException.
    /// The null-token guard runs before the try/catch in ConverterBase.Read and is not wrapped.
    /// </summary>
    [Fact]
    public void ConverterBase_NullToken_NonNullableTarget_Throws()
    {
        Wrap.It(() => JsonSerializer.Deserialize<Instant>("null", With(Converters.InstantConverter)))
            .Throws<InvalidNodaDataException>();
    }

    // -------------------------------------------------------------------------
    // ConverterBase<T> — empty-string token
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that an empty JSON string deserializing into a non-nullable Instant throws InvalidNodaDataException.
    /// The empty-string guard runs before the try/catch in ConverterBase.Read and is not wrapped.
    /// </summary>
    [Fact]
    public void ConverterBase_EmptyString_NonNullableTarget_Throws()
    {
        Wrap.It(() => JsonSerializer.Deserialize<Instant>("\"\"", With(Converters.InstantConverter)))
            .Throws<InvalidNodaDataException>();
    }

    // -------------------------------------------------------------------------
    // NodaPatternConverter<T> — non-String token
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that a JSON number token deserializing as Instant throws JsonException.
    /// NodaPatternConverter.ReadImplementation is called from inside ConverterBase's try/catch,
    /// so the InvalidNodaDataException it raises gets wrapped into a JsonException.
    /// </summary>
    [Fact]
    public void NodaPatternConverter_NonStringToken_Throws()
    {
        Wrap.It(() => JsonSerializer.Deserialize<Instant>("42", With(Converters.InstantConverter)))
            .Throws<JsonException>();
    }

    // -------------------------------------------------------------------------
    // NodaDateIntervalConverter — missing properties
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that a JSON object missing the "start" property throws JsonException when deserializing a DateInterval.
    /// </summary>
    [Fact]
    public void NodaDateIntervalConverter_MissingStart_Throws()
    {
        var json = "{\"end\":\"2013-06-07\"}";
        Wrap.It(() =>
                JsonSerializer.Deserialize<DateInterval>(
                    json,
                    With(Converters.DateIntervalConverter, Converters.LocalDateConverter)
                )
            )
            .Throws<JsonException>();
    }

    /// <summary>
    /// Tests that a JSON object missing the "end" property throws JsonException when deserializing a DateInterval.
    /// </summary>
    [Fact]
    public void NodaDateIntervalConverter_MissingEnd_Throws()
    {
        var json = "{\"start\":\"2012-01-02\"}";
        Wrap.It(() =>
                JsonSerializer.Deserialize<DateInterval>(
                    json,
                    With(Converters.DateIntervalConverter, Converters.LocalDateConverter)
                )
            )
            .Throws<JsonException>();
    }

    // -------------------------------------------------------------------------
    // NodaIsoDateIntervalConverter — error branches
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that a JSON number token (non-String) throws JsonException when deserializing a DateInterval with the ISO converter.
    /// </summary>
    [Fact]
    public void NodaIsoDateIntervalConverter_NonStringToken_Throws()
    {
        Wrap.It(() => JsonSerializer.Deserialize<DateInterval>("42", With(Converters.IsoDateIntervalConverter)))
            .Throws<JsonException>();
    }

    /// <summary>
    /// Tests that a string without a slash throws JsonException when deserializing a DateInterval with the ISO converter.
    /// </summary>
    [Fact]
    public void NodaIsoDateIntervalConverter_NoSlash_Throws()
    {
        Wrap.It(() =>
                JsonSerializer.Deserialize<DateInterval>(
                    "\"2012-01-022013-06-07\"",
                    With(Converters.IsoDateIntervalConverter)
                )
            )
            .Throws<JsonException>();
    }

    /// <summary>
    /// Tests that a string with an empty start part throws JsonException when deserializing a DateInterval with the ISO converter.
    /// </summary>
    [Fact]
    public void NodaIsoDateIntervalConverter_EmptyStart_Throws()
    {
        Wrap.It(() =>
                JsonSerializer.Deserialize<DateInterval>("\"/2013-06-07\"", With(Converters.IsoDateIntervalConverter))
            )
            .Throws<JsonException>();
    }

    /// <summary>
    /// Tests that a string with an empty end part throws JsonException when deserializing a DateInterval with the ISO converter.
    /// </summary>
    [Fact]
    public void NodaIsoDateIntervalConverter_EmptyEnd_Throws()
    {
        Wrap.It(() =>
                JsonSerializer.Deserialize<DateInterval>("\"2012-01-02/\"", With(Converters.IsoDateIntervalConverter))
            )
            .Throws<JsonException>();
    }

    // -------------------------------------------------------------------------
    // NodaIsoIntervalConverter — error branches
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that a JSON number token (non-String) throws JsonException when deserializing an Interval with the ISO converter.
    /// </summary>
    [Fact]
    public void NodaIsoIntervalConverter_NonStringToken_Throws()
    {
        Wrap.It(() => JsonSerializer.Deserialize<Interval>("42", With(Converters.IsoIntervalConverter)))
            .Throws<JsonException>();
    }

    /// <summary>
    /// Tests that a string without a slash throws JsonException when deserializing an Interval with the ISO converter.
    /// </summary>
    [Fact]
    public void NodaIsoIntervalConverter_NoSlash_Throws()
    {
        Wrap.It(() =>
                JsonSerializer.Deserialize<Interval>(
                    "\"2012-01-02T03:04:05Z2013-06-07T08:09:10Z\"",
                    With(Converters.IsoIntervalConverter)
                )
            )
            .Throws<JsonException>();
    }
}
