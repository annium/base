using Annium.Core.Runtime.Types;

namespace Annium.Net.Types.Refs;

/// <summary>
/// Type reference for primitive or base types like int, string, bool, etc.
/// </summary>
/// <param name="Name">The name of the base type</param>
[ResolutionKeyValue(RefType.BaseType)]
public sealed record BaseTypeRef(string Name) : IRef
{
    /// <summary>
    /// Gets the type of reference this represents.
    /// </summary>
    public RefType Type => RefType.BaseType;
}
