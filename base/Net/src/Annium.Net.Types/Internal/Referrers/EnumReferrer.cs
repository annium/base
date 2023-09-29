using Annium.Logging;
using Annium.Net.Types.Refs;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Referrers;

internal class EnumReferrer : IReferrer
{
    public ILogger Logger { get; }

    public EnumReferrer(ILogger logger)
    {
        Logger = logger;
    }

    public IRef? GetRef(ContextualType type, Nullability nullability, IProcessingContext ctx)
    {
        return type.Type.IsEnum ? ctx.RequireRef(type) : null;
    }
}