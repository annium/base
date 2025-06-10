using Annium.Core.Runtime.Types;

namespace Annium.Net.Types.Refs;

/// <summary>
/// Type reference for nullable wrapper types like int?, string?, etc.
/// Wraps the underlying value type reference.
/// </summary>
/// <param name="Value">The type reference for the nullable value type</param>
[ResolutionKeyValue(RefType.Nullable)]
public sealed record NullableRef(IRef Value) : IRef
{
    /// <summary>
    /// Gets the type of reference this represents.
    /// </summary>
    public RefType Type => RefType.Nullable;
}
