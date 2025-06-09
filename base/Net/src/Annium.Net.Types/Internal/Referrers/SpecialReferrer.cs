using System;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Net.Types.Internal.Extensions;
using Annium.Net.Types.Refs;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Referrers;

/// <summary>
/// Referrer that creates references for special types like Task, ValueTask, and Promise types.
/// </summary>
internal class SpecialReferrer : IReferrer
{
    /// <summary>
    /// Gets the logger for this referrer.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Initializes a new instance of the SpecialReferrer.
    /// </summary>
    /// <param name="logger">The logger to use for this referrer</param>
    public SpecialReferrer(ILogger logger)
    {
        Logger = logger;
    }

    /// <summary>
    /// Gets a type reference for the specified contextual type if it is a special type.
    /// </summary>
    /// <param name="type">The contextual type to get a reference for</param>
    /// <param name="nullability">The nullability information for the type</param>
    /// <param name="ctx">The processing context</param>
    /// <returns>A promise reference for Task/ValueTask types, null otherwise</returns>
    public IRef? GetRef(ContextualType type, Nullability nullability, IProcessingContext ctx)
    {
        return type.Type.IsGenericType
            ? ProcessGeneric(type, type.Type.GetGenericTypeDefinition(), ctx)
            : ProcessNonGeneric(type);
    }

    /// <summary>
    /// Processes generic Task and ValueTask types to create promise references.
    /// </summary>
    /// <param name="type">The contextual type to process</param>
    /// <param name="definition">The generic type definition</param>
    /// <param name="ctx">The processing context</param>
    /// <returns>A promise reference if the type is a generic task type, null otherwise</returns>
    private static IRef? ProcessGeneric(ContextualType type, Type definition, IProcessingContext ctx)
    {
        if (definition == typeof(Task<>) || definition == typeof(ValueTask<>))
        {
            var typeGenericArguments = type.GetGenericArguments();
            return new PromiseRef(ctx.GetRef(typeGenericArguments[0]));
        }

        return null;
    }

    /// <summary>
    /// Processes non-generic Task and ValueTask types to create promise references.
    /// </summary>
    /// <param name="type">The contextual type to process</param>
    /// <returns>A promise reference if the type is a non-generic task type, null otherwise</returns>
    private static IRef? ProcessNonGeneric(ContextualType type)
    {
        if (type.Type == typeof(Task) || type.Type == typeof(ValueTask))
            return new PromiseRef(null);

        return null;
    }
}
