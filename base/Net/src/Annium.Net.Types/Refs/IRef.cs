using Annium.Core.Runtime.Types;

namespace Annium.Net.Types.Refs;

/// <summary>
/// Base interface for all type references in the type mapping system.
/// Provides a type identifier for runtime resolution and dispatching.
/// </summary>
public interface IRef
{
    /// <summary>
    /// Gets the type of reference this represents for runtime resolution.
    /// </summary>
    [ResolutionKey]
    RefType Type { get; }
}
