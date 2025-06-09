using Annium.Logging;
using Annium.Net.Types.Refs;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Referrers;

/// <summary>
/// Referrer that creates references for enumeration types.
/// </summary>
internal class EnumReferrer : IReferrer
{
    /// <summary>
    /// Gets the logger for this referrer.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Initializes a new instance of the EnumReferrer.
    /// </summary>
    /// <param name="logger">The logger to use for this referrer</param>
    public EnumReferrer(ILogger logger)
    {
        Logger = logger;
    }

    /// <summary>
    /// Gets a type reference for the specified contextual type if it is an enumeration.
    /// </summary>
    /// <param name="type">The contextual type to get a reference for</param>
    /// <param name="nullability">The nullability information for the type</param>
    /// <param name="ctx">The processing context</param>
    /// <returns>An enum reference if the type is an enum, null otherwise</returns>
    public IRef? GetRef(ContextualType type, Nullability nullability, IProcessingContext ctx)
    {
        return type.Type.IsEnum ? ctx.RequireRef(type) : null;
    }
}
