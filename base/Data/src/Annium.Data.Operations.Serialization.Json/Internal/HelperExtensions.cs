using System;
using System.Text.Json;

namespace Annium.Data.Operations.Serialization.Json.Internal;

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