using System;
using System.Collections.Generic;
using Annium.Logging;
using Annium.Net.Types.Internal.Extensions;
using Annium.Net.Types.Refs;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Referrers;

/// <summary>
/// Main referrer that coordinates type reference creation by delegating to specific referrer implementations.
/// </summary>
internal class Referrer : ILogSubject
{
    /// <summary>
    /// Gets the logger for this referrer.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// The collection of specific referrers to delegate to.
    /// </summary>
    private readonly IEnumerable<IReferrer> _referrers;

    /// <summary>
    /// Initializes a new instance of the Referrer with the specified referrers and logger.
    /// </summary>
    /// <param name="referrers">The collection of referrers to delegate to</param>
    /// <param name="logger">The logger to use for this referrer</param>
    public Referrer(IEnumerable<IReferrer> referrers, ILogger logger)
    {
        _referrers = referrers;
        Logger = logger;
    }

    /// <summary>
    /// Gets a type reference for a contextual type using its inherent nullability information.
    /// </summary>
    /// <param name="type">The contextual type to get a reference for</param>
    /// <param name="ctx">The processing context</param>
    /// <returns>A type reference for the contextual type</returns>
    public IRef GetRef(ContextualType type, IProcessingContext ctx) => GetRef(type, type.Nullability, ctx);

    /// <summary>
    /// Gets a type reference for a contextual type with explicit nullability by delegating to appropriate referrers.
    /// </summary>
    /// <param name="type">The contextual type to get a reference for</param>
    /// <param name="nullability">The nullability information for the type</param>
    /// <param name="ctx">The processing context</param>
    /// <returns>A type reference for the contextual type</returns>
    public IRef GetRef(ContextualType type, Nullability nullability, IProcessingContext ctx)
    {
        this.Trace<string>("Resolve {type} ref", type.FriendlyName());

        foreach (var referrer in _referrers)
        {
            var result = referrer.GetRef(type, nullability, ctx);
            if (result is null)
                continue;

            this.Trace<string, IRef?, string>(
                "Resolved {type} ref as {result} via {referrerType}",
                type.FriendlyName(),
                result,
                referrer.GetType().FriendlyName()
            );
            return result;
        }

        throw new InvalidOperationException($"Failed to resolve {type.FriendlyName()} ref");
    }
}
