using Annium.Logging;
using Annium.Net.Types.Internal.Helpers;
using Annium.Net.Types.Refs;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Referrers;

/// <summary>
/// Referrer that creates references for types that have been excluded from mapping.
/// </summary>
internal class ExcludedReferrer : IReferrer
{
    /// <summary>
    /// Factory method for creating struct references for excluded types.
    /// </summary>
    /// <param name="ns">The namespace</param>
    /// <param name="name">The type name</param>
    /// <param name="args">The generic arguments</param>
    /// <returns>A new struct reference</returns>
    private static StructRef BuildStructRef(string ns, string name, IRef[] args) => new(ns, name, args);

    /// <summary>
    /// Factory method for creating interface references for excluded types.
    /// </summary>
    /// <param name="ns">The namespace</param>
    /// <param name="name">The type name</param>
    /// <param name="args">The generic arguments</param>
    /// <returns>A new interface reference</returns>
    private static InterfaceRef BuildInterfaceRef(string ns, string name, IRef[] args) => new(ns, name, args);

    /// <summary>
    /// Factory method for creating enum references for excluded types.
    /// </summary>
    /// <param name="ns">The namespace</param>
    /// <param name="name">The type name</param>
    /// <param name="args">The generic arguments (unused for enums)</param>
    /// <returns>A new enum reference</returns>
    private static EnumRef BuildEnumRef(string ns, string name, IRef[] args) => new(ns, name);

    /// <summary>
    /// Gets the logger for this referrer.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Initializes a new instance of the ExcludedReferrer.
    /// </summary>
    /// <param name="logger">The logger to use for this referrer</param>
    public ExcludedReferrer(ILogger logger)
    {
        Logger = logger;
    }

    /// <summary>
    /// Gets a type reference for the specified contextual type if it is excluded from mapping.
    /// </summary>
    /// <param name="type">The contextual type to get a reference for</param>
    /// <param name="nullability">The nullability information for the type</param>
    /// <param name="ctx">The processing context</param>
    /// <returns>A type reference for the excluded type, or null if not excluded</returns>
    public IRef? GetRef(ContextualType type, Nullability nullability, IProcessingContext ctx)
    {
        if (!ctx.Config.IsExcluded(type))
            return null;

        if (type.Type.IsEnum)
            return this.BuildRef(type, ctx, BuildEnumRef);

        if (type.Type.IsInterface)
            return this.BuildRef(type, ctx, BuildInterfaceRef);

        return this.BuildRef(type, ctx, BuildStructRef);
    }
}
