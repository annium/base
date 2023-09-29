using System;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Net.Types.Internal.Extensions;
using Annium.Net.Types.Refs;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Referrers;

internal class SpecialReferrer : IReferrer
{
    public ILogger Logger { get; }

    public SpecialReferrer(ILogger logger)
    {
        Logger = logger;
    }

    public IRef? GetRef(ContextualType type, Nullability nullability, IProcessingContext ctx)
    {
        return type.Type.IsGenericType
            ? ProcessGeneric(type, type.Type.GetGenericTypeDefinition(), ctx)
            : ProcessNonGeneric(type);
    }

    private static IRef? ProcessGeneric(ContextualType type, Type definition, IProcessingContext ctx)
    {
        if (definition == typeof(Task<>) || definition == typeof(ValueTask<>))
        {
            var typeGenericArguments = type.GetGenericArguments();
            return new PromiseRef(ctx.GetRef(typeGenericArguments[0]));
        }

        return null;
    }

    private static IRef? ProcessNonGeneric(ContextualType type)
    {
        if (type.Type == typeof(Task) || type.Type == typeof(ValueTask))
            return new PromiseRef(null);

        return null;
    }
}