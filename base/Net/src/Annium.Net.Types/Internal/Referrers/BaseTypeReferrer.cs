using Annium.Logging;
using Annium.Net.Types.Refs;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Referrers;

/// <summary>
/// Referrer that creates references for types that have been registered as base types.
/// </summary>
internal class BaseTypeReferrer : IReferrer
{
    /// <summary>
    /// Gets the logger for this referrer.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Initializes a new instance of the BaseTypeReferrer.
    /// </summary>
    /// <param name="logger">The logger to use for this referrer</param>
    public BaseTypeReferrer(ILogger logger)
    {
        Logger = logger;
    }

    /// <summary>
    /// Gets a type reference for the specified contextual type if it is a registered base type.
    /// </summary>
    /// <param name="type">The contextual type to get a reference for</param>
    /// <param name="nullability">The nullability information for the type</param>
    /// <param name="ctx">The processing context</param>
    /// <returns>A base type reference if the type is registered, null otherwise</returns>
    public IRef? GetRef(ContextualType type, Nullability nullability, IProcessingContext ctx)
    {
        return ctx.Config.GetBaseTypeRefFor(type.Type);
    }
}
