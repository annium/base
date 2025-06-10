using Annium.Core.Runtime.Types;

namespace Annium.Net.Types.Refs;

/// <summary>
/// Type reference for asynchronous promise or task-like types.
/// Wraps the result type reference to indicate asynchronous semantics.
/// </summary>
/// <param name="Value">The type reference for the promise result type, or null for void promises</param>
[ResolutionKeyValue(RefType.Promise)]
public sealed record PromiseRef(IRef? Value) : IRef
{
    /// <summary>
    /// Gets the type of reference this represents.
    /// </summary>
    public RefType Type => RefType.Promise;
}
