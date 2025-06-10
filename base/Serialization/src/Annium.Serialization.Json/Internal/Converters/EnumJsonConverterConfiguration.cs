namespace Annium.Serialization.Json.Internal.Converters;

/// <summary>
/// Configuration for enum JSON converters.
/// </summary>
/// <typeparam name="T">The enum type.</typeparam>
internal class EnumJsonConverterConfiguration<T>
    where T : struct
{
    /// <summary>
    /// Gets the separator used for flags enum values.
    /// </summary>
    public string ValueSeparator { get; }

    /// <summary>
    /// Gets the default value to use when parsing fails.
    /// </summary>
    public T? DefaultValue { get; }

    /// <summary>
    /// Initializes a new instance of the EnumJsonConverterConfiguration class.
    /// </summary>
    /// <param name="valueSeparator">The separator used for flags enum values.</param>
    /// <param name="defaultValue">The default value to use when parsing fails.</param>
    public EnumJsonConverterConfiguration(string valueSeparator, T? defaultValue)
    {
        ValueSeparator = valueSeparator;
        DefaultValue = defaultValue;
    }
}
