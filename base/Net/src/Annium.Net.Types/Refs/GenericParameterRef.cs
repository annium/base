using Annium.Core.Runtime.Types;

namespace Annium.Net.Types.Refs;

/// <summary>
/// Type reference for generic type parameters like T, TKey, TValue, etc.
/// Represents unresolved generic parameters in type definitions.
/// </summary>
/// <param name="Name">The name of the generic parameter</param>
[ResolutionKeyValue(RefType.GenericParameter)]
public sealed record GenericParameterRef(string Name) : IRef
{
    /// <summary>
    /// Gets the type of reference this represents.
    /// </summary>
    public RefType Type => RefType.GenericParameter;
}
