using System;
using System.Collections.Generic;
using Annium.Logging;
using Annium.Net.Types.Internal.Extensions;
using Annium.Net.Types.Refs;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Referrers;

internal class Referrer : ILogSubject
{
    public ILogger Logger { get; }
    private readonly IEnumerable<IReferrer> _referrers;

    public Referrer(
        IEnumerable<IReferrer> referrers,
        ILogger logger
    )
    {
        _referrers = referrers;
        Logger = logger;
    }

    public IRef GetRef(ContextualType type, IProcessingContext ctx) => GetRef(type, type.Nullability, ctx);

    public IRef GetRef(ContextualType type, Nullability nullability, IProcessingContext ctx)
    {
        this.Trace<string>("Resolve {type} ref", type.FriendlyName());

        foreach (var referrer in _referrers)
        {
            var result = referrer.GetRef(type, nullability, ctx);
            if (result is null)
                continue;

            this.Trace<string, IRef?, string>("Resolved {type} ref as {result} via {referrerType}", type.FriendlyName(), result, referrer.GetType().FriendlyName());
            return result;
        }

        throw new InvalidOperationException($"Failed to resolve {type.FriendlyName()} ref");
    }
}