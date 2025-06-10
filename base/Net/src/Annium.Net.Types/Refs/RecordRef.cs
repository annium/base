using Annium.Core.Runtime.Types;

namespace Annium.Net.Types.Refs;

/// <summary>
/// Type reference for record types, representing key-value pair structures.
/// Used for dictionary-like or tuple record types with distinct key and value types.
/// </summary>
/// <param name="Key">The type reference for the key type</param>
/// <param name="Value">The type reference for the value type</param>
[ResolutionKeyValue(RefType.Record)]
public sealed record RecordRef(IRef Key, IRef Value) : IRef
{
    /// <summary>
    /// Gets the type of reference this represents.
    /// </summary>
    public RefType Type => RefType.Record;
}
