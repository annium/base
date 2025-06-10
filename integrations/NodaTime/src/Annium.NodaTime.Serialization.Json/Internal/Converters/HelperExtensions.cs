using System;
using System.Text.Json;

namespace Annium.NodaTime.Serialization.Json.Internal.Converters;

/// <summary>
/// Helper extension methods for JSON reader operations.
/// </summary>
internal static class HelperExtensions
{
    /// <summary>
    /// Determines whether the current JSON reader position represents a property with the specified name (case-insensitive).
    /// </summary>
    /// <param name="reader">The JSON reader to check.</param>
    /// <param name="property">The property name to match.</param>
    /// <returns>true if the current token is a property name that matches the specified property; otherwise, false.</returns>
    public static bool HasProperty(this ref Utf8JsonReader reader, string property)
    {
        if (reader.TokenType != JsonTokenType.PropertyName)
            return false;

        var name = reader.GetString()!;

        return name.Equals(property, StringComparison.InvariantCultureIgnoreCase);
    }
}
