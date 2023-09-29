using System;
using System.Linq;
using Annium.Net.Types.Internal.Config;
using Annium.Net.Types.Internal.Extensions;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Helpers;

internal static class ArrayHelper
{
    public static ContextualType ResolveElementType(ContextualType type)
    {
        ContextualType? elementType;
        if (type.Type.IsArray)
            elementType = type.Type.GetElementType()?.ToContextualType();
        else if (type.Type.IsGenericType && type.Type.GetGenericTypeDefinition() == MapperConfig.BaseArrayType)
            elementType = type.GetGenericArguments()[0];
        else
        {
            var arrayImplementation = type.GetInterfaces()
                .SingleOrDefault(x => x.Type.IsGenericType && x.Type.GetGenericTypeDefinition() == MapperConfig.BaseArrayType);
            elementType = arrayImplementation?.GetGenericArguments()[0];
        }

        return elementType ?? throw new InvalidOperationException($"Failed to resolve element type of {type.FriendlyName()}");
    }
}