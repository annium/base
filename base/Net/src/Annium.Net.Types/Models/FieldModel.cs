using Annium.Net.Types.Refs;

namespace Annium.Net.Types.Models;

/// <summary>
/// Represents a field or property within a type model.
/// Contains the field's type reference and name.
/// </summary>
/// <param name="Type">The type reference for this field</param>
/// <param name="Name">The name of this field</param>
public sealed record FieldModel(IRef Type, string Name)
{
    /// <summary>
    /// Returns a string representation of this field model.
    /// </summary>
    /// <returns>A string in the format "Type Name"</returns>
    public override string ToString() => $"{Type} {Name}";
}
