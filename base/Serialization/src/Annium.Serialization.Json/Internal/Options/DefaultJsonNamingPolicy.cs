using System.Text.Json;

namespace Annium.Serialization.Json.Internal.Options;

/// <summary>
/// A JSON naming policy that preserves property names as-is without any transformation.
/// </summary>
internal class DefaultJsonNamingPolicy : JsonNamingPolicy
{
    /// <summary>
    /// Converts the specified name according to the naming policy.
    /// </summary>
    /// <param name="name">The name to convert.</param>
    /// <returns>The name unchanged.</returns>
    public override string ConvertName(string name) => name;
}
