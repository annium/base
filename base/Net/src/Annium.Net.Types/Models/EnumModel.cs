using System.Collections.Generic;

namespace Annium.Net.Types.Models;

/// <summary>
/// Represents an enumeration type model containing namespace, name, and enumeration values.
/// </summary>
/// <param name="Namespace">The namespace containing this enum</param>
/// <param name="Name">The name of this enum</param>
/// <param name="Values">The enumeration values mapped from names to their numeric values</param>
public sealed record EnumModel(Namespace Namespace, string Name, IReadOnlyDictionary<string, long> Values) : IModel
{
    /// <summary>
    /// Returns a string representation of this enum model.
    /// </summary>
    /// <returns>A string in the format "enum Namespace.Name"</returns>
    public override string ToString() => $"enum {Namespace}.{Name}";
}
