using System;
using System.Linq;
using Annium.Logging;
using Annium.Net.Types.Internal.Extensions;
using Annium.Net.Types.Internal.Referrers;
using Annium.Net.Types.Refs;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Helpers;

internal static class ReferrerExtensions
{
    public static TRef BuildRef<TRef>(this IReferrer referrer, ContextualType type, IProcessingContext ctx, Func<string, string, IRef[], TRef> factory)
        where TRef : IModelRef
    {
        var name = type.PureName();
        var typeGenericArguments = type.GetGenericArguments();
        var genericArguments = typeGenericArguments.Select(ctx.GetRef).ToArray();
        var modelRef = factory(type.Type.Namespace ?? string.Empty, name, genericArguments);
        referrer.Trace("Created {type} ref as {modelRef}", type.FriendlyName(), modelRef);

        return modelRef;
    }
}