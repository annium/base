using Annium.Core.Runtime.Types;

namespace Annium.Net.Types.Refs;

/// <summary>
/// Type reference representing an array or collection type.
/// Wraps the element type reference to indicate array semantics.
/// </summary>
/// <param name="Value">The type reference for the array element type</param>
[ResolutionKeyValue(RefType.Array)]
public sealed record ArrayRef(IRef Value) : IRef
{
    /// <summary>
    /// Gets the type of reference this represents.
    /// </summary>
    public RefType Type => RefType.Array;
}
