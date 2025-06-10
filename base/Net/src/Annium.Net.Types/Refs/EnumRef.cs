using Annium.Core.Runtime.Types;

namespace Annium.Net.Types.Refs;

/// <summary>
/// Type reference for enumeration types.
/// Implements IModelRef to provide namespace and name identification.
/// </summary>
/// <param name="Namespace">The namespace containing the enum</param>
/// <param name="Name">The name of the enum</param>
[ResolutionKeyValue(RefType.Enum)]
public sealed record EnumRef(string Namespace, string Name) : IModelRef
{
    /// <summary>
    /// Gets the type of reference this represents.
    /// </summary>
    public RefType Type => RefType.Enum;
}
