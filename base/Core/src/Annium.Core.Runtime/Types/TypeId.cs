using System;
using System.Linq;
using Annium.Core.Runtime.Internal.Types;

namespace Annium.Core.Runtime.Types;

public abstract record TypeId
{
    public static TypeId Create(Type type)
    {
        if (type.IsGenericParameter)
            throw new InvalidOperationException("TypeId is not valid for generic parameters");

        if (!type.IsGenericType)
            return new NonGenericTypeId(type);

        if (type.IsGenericTypeDefinition)
            return new OpenGenericTypeId(type);

        return new ClosedGenericTypeId(type);
    }

    public static TypeId? TryParse(string id, ITypeManager tm)
    {
        // if not constructed generic type - try resolve from TypeManager
        if (!id.Contains('<') && !id.Contains('>'))
            return tm.GetTypeId(id);

        var laIndex = id.IndexOf('<');
        var raIndex = id.LastIndexOf('>');
        if (laIndex < 0 || raIndex < 0 || raIndex < laIndex)
            throw new ArgumentException($"String id '{id}' has invalid format");

        var name = id[..laIndex];
        var baseId = tm.GetTypeId(name);
        if (baseId is null)
            return null;

        var rawArgs = id.Substring(laIndex + 1, raIndex - laIndex - 1).Split(',');
        var args = rawArgs.Select(x => TryParse(x, tm)).ToArray();
        if (args.Any(x => x is null))
            return null;

        var type = baseId.Type.MakeGenericType(args.Select(x => x!.Type).ToArray());

        return Create(type);
    }

    protected static string GetName(Type type)
    {
        var ns = type.Namespace;
        var name = type.Name;

        return $"{ns}:{name}";
    }

    public Type Type { get; }
    public Type BaseType { get; }
    public string Name { get; }
    public string Id { get; protected init; } = string.Empty;

    protected TypeId(
        Type type,
        string name
    ) : this(
        type,
        type,
        name
    )
    {
    }

    protected TypeId(
        Type type,
        Type baseType,
        string name
    )
    {
        Type = type;
        BaseType = baseType;
        Name = name;
    }

    public virtual bool Equals(TypeId? other) => Id == other?.Id;

    public override int GetHashCode() => Id.GetHashCode();
}