using System;
using System.Text.Json;

namespace Annium.NodaTime.Serialization.Json.Internal.Converters;

internal static class HelperExtensions
{
    public static bool HasProperty(this ref Utf8JsonReader reader, string property)
    {
        if (reader.TokenType != JsonTokenType.PropertyName)
            return false;

        var name = reader.GetString()!;

        return name.Equals(property, StringComparison.InvariantCultureIgnoreCase);
    }
}