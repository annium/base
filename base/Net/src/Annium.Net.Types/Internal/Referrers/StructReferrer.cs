using Annium.Logging;
using Annium.Net.Types.Internal.Helpers;
using Annium.Net.Types.Refs;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Referrers;

/// <summary>
/// Referrer that creates references for struct types.
/// </summary>
internal class StructReferrer : IReferrer
{
    /// <summary>
    /// Gets the logger for this referrer.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Initializes a new instance of the StructReferrer.
    /// </summary>
    /// <param name="logger">The logger to use for this referrer</param>
    public StructReferrer(ILogger logger)
    {
        Logger = logger;
    }

    /// <summary>
    /// Gets a type reference for the specified contextual type, treating it as a struct.
    /// </summary>
    /// <param name="type">The contextual type to get a reference for</param>
    /// <param name="nullability">The nullability information for the type</param>
    /// <param name="ctx">The processing context</param>
    /// <returns>A struct reference for the type</returns>
    public IRef GetRef(ContextualType type, Nullability nullability, IProcessingContext ctx)
    {
        var modelRef = this.BuildRef(type, ctx, static (ns, name, args) => new StructRef(ns, name, args));

        return modelRef;
    }
}
