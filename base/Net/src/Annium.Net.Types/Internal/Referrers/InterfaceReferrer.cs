using Annium.Logging;
using Annium.Net.Types.Internal.Helpers;
using Annium.Net.Types.Refs;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Referrers;

internal class InterfaceReferrer : IReferrer
{
    public ILogger Logger { get; }

    public InterfaceReferrer(ILogger logger)
    {
        Logger = logger;
    }

    public IRef? GetRef(ContextualType type, Nullability nullability, IProcessingContext ctx)
    {
        if (!type.Type.IsInterface)
            return null;

        var modelRef = this.BuildRef(type, ctx, static (ns, name, args) => new InterfaceRef(ns, name, args));

        return modelRef;
    }
}