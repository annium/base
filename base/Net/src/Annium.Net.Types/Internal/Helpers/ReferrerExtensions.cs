using System;
using System.Linq;
using Annium.Logging;
using Annium.Net.Types.Internal.Extensions;
using Annium.Net.Types.Internal.Referrers;
using Annium.Net.Types.Refs;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Helpers;

/// <summary>
/// Extension methods for referrers that provide common functionality for building type references.
/// </summary>
internal static class ReferrerExtensions
{
    /// <summary>
    /// Builds a type reference for the specified contextual type using the provided factory function.
    /// </summary>
    /// <typeparam name="TRef">The type of reference to build</typeparam>
    /// <param name="referrer">The referrer instance</param>
    /// <param name="type">The contextual type to build a reference for</param>
    /// <param name="ctx">The processing context</param>
    /// <param name="factory">Factory function to create the reference</param>
    /// <returns>The built type reference</returns>
    public static TRef BuildRef<TRef>(
        this IReferrer referrer,
        ContextualType type,
        IProcessingContext ctx,
        Func<string, string, IRef[], TRef> factory
    )
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
