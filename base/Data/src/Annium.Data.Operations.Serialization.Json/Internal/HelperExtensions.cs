using System;
using System.Text.Json;

namespace Annium.Data.Operations.Serialization.Json.Internal;

/// <summary>
/// Helper extension methods for JSON operations.
/// </summary>
internal static class HelperExtensions
{
    /// <summary>
    /// Checks if the current JSON token is a property with the specified name.
    /// </summary>
    /// <param name="reader">The JSON reader.</param>
    /// <param name="property">The property name to check for.</param>
    /// <returns>True if the current token is a property with the specified name, false otherwise.</returns>
    public static bool HasProperty(this ref Utf8JsonReader reader, string property)
    {
        if (reader.TokenType != JsonTokenType.PropertyName)
            return false;

        var name = reader.GetString()!;

        return name.Equals(property, StringComparison.InvariantCultureIgnoreCase);
    }
}
