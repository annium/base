using Annium.Logging;
using Annium.Net.Types.Refs;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Referrers;

/// <summary>
/// Interface for type referrers that create type references for specific scenarios.
/// Referrers determine whether they can create a reference for a type and return the appropriate reference.
/// </summary>
internal interface IReferrer : ILogSubject
{
    /// <summary>
    /// Gets a type reference for the specified contextual type if this referrer can handle it.
    /// </summary>
    /// <param name="type">The contextual type to create a reference for</param>
    /// <param name="nullability">The nullability context</param>
    /// <param name="ctx">The processing context</param>
    /// <returns>A type reference if this referrer can handle the type, null otherwise</returns>
    IRef? GetRef(ContextualType type, Nullability nullability, IProcessingContext ctx);
}
