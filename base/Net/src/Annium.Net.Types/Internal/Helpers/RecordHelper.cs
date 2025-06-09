using System;
using System.Linq;
using Annium.Net.Types.Internal.Config;
using Annium.Net.Types.Internal.Extensions;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Helpers;

/// <summary>
/// Helper methods for working with record types and resolving their key-value element types.
/// </summary>
internal static class RecordHelper
{
    /// <summary>
    /// Resolves the key and value types for a record type that implements IEnumerable&lt;KeyValuePair&lt;TKey, TValue&gt;&gt;.
    /// </summary>
    /// <param name="type">The contextual type to resolve element types for</param>
    /// <returns>A tuple containing the key type and value type of the record</returns>
    public static (ContextualType keyType, ContextualType valueType) ResolveElementType(ContextualType type)
    {
        var arrayImplementation =
            type.Type.IsGenericType && type.Type.GetGenericTypeDefinition() == MapperConfig.BaseArrayType
                ? type
                : type.GetInterfaces()
                    .SingleOrDefault(x =>
                        x.Type.IsGenericType && x.Type.GetGenericTypeDefinition() == MapperConfig.BaseArrayType
                    );
        if (arrayImplementation is null)
            throw new InvalidOperationException(
                $"Type {type.FriendlyName()} doesn't implement {MapperConfig.BaseArrayType.FriendlyName()}"
            );

        var elementType = arrayImplementation.GetGenericArguments()[0];
        if (
            !elementType.Type.IsGenericType
            || elementType.Type.GetGenericTypeDefinition() != MapperConfig.BaseRecordValueType
        )
            throw new InvalidOperationException(
                $"Type {type.FriendlyName()} element type doesn't implement {MapperConfig.BaseRecordValueType.FriendlyName()}"
            );

        var elementTypeArguments = elementType.GetGenericArguments();

        return (elementTypeArguments[0], elementTypeArguments[1]);
    }
}
