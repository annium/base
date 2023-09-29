using Annium.Logging;
using Annium.Net.Types.Refs;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Referrers;

internal class GenericParameterReferrer : IReferrer
{
    public ILogger Logger { get; }

    public GenericParameterReferrer(ILogger logger)
    {
        Logger = logger;
    }

    public IRef? GetRef(ContextualType type, Nullability nullability, IProcessingContext ctx)
    {
        return type.Type.IsGenericParameter ? new GenericParameterRef(type.Type.Name) : null;
    }
}