using Annium.Logging;
using Annium.Net.Types.Refs;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Referrers;

internal class BaseTypeReferrer : IReferrer
{
    public ILogger Logger { get; }

    public BaseTypeReferrer(ILogger logger)
    {
        Logger = logger;
    }

    public IRef? GetRef(ContextualType type, Nullability nullability, IProcessingContext ctx)
    {
        return ctx.Config.GetBaseTypeRefFor(type.Type);
    }
}