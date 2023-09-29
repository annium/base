using System;
using System.Reflection;
using Annium.Core.Runtime.Types;

namespace Annium.Core.Runtime.Internal.Types;

internal sealed class Descendant
{
    public Type Type { get; }
    public TypeSignature Signature { get; }
    public TypeId Id { get; }
    public object? Key { get; }
    public bool HasKey { get; }

    public Descendant(
        Type type
    )
    {
        Type = type;
        Signature = TypeSignature.Create(type);
        Id = type.GetTypeId();
        var keyAttribute = type.GetTypeInfo().GetCustomAttribute<ResolutionKeyValueAttribute>();
        Key = keyAttribute?.Key;
        HasKey = keyAttribute != null;
    }

    public override string ToString() => Type.Name;
}