namespace Annium.Serialization.Json.Internal.Converters;

internal class EnumJsonConverterConfiguration<T>
    where T : struct
{
    public string ValueSeparator { get; }
    public T? DefaultValue { get; }

    public EnumJsonConverterConfiguration(
        string valueSeparator,
        T? defaultValue
    )
    {
        ValueSeparator = valueSeparator;
        DefaultValue = defaultValue;
    }
}