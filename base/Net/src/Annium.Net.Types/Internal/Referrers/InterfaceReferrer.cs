using Annium.Logging;
using Annium.Net.Types.Internal.Helpers;
using Annium.Net.Types.Refs;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Referrers;

/// <summary>
/// Referrer that creates references for interface types.
/// </summary>
internal class InterfaceReferrer : IReferrer
{
    /// <summary>
    /// Gets the logger for this referrer.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Initializes a new instance of the InterfaceReferrer.
    /// </summary>
    /// <param name="logger">The logger to use for this referrer</param>
    public InterfaceReferrer(ILogger logger)
    {
        Logger = logger;
    }

    /// <summary>
    /// Gets a type reference for the specified contextual type if it is an interface.
    /// </summary>
    /// <param name="type">The contextual type to get a reference for</param>
    /// <param name="nullability">The nullability information for the type</param>
    /// <param name="ctx">The processing context</param>
    /// <returns>An interface reference if the type is an interface, null otherwise</returns>
    public IRef? GetRef(ContextualType type, Nullability nullability, IProcessingContext ctx)
    {
        if (!type.Type.IsInterface)
            return null;

        var modelRef = this.BuildRef(type, ctx, static (ns, name, args) => new InterfaceRef(ns, name, args));

        return modelRef;
    }
}
