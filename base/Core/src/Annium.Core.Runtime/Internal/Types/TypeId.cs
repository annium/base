using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Core.Runtime.Types;

namespace Annium.Core.Runtime.Internal.Types;

internal sealed record NonGenericTypeId : TypeId
{
    public NonGenericTypeId(
        Type type
    ) : base(
        type,
        GetName(type)
    )
    {
        Id = Name;
    }

    public bool Equals(NonGenericTypeId? other) => Id == other?.Id;

    public override int GetHashCode() => Id.GetHashCode();
}

internal sealed record OpenGenericTypeId : TypeId
{
    public OpenGenericTypeId(
        Type type
    ) : base(
        type,
        type,
        GetName(type)
    )
    {
        Id = GetName(type);
    }

    public bool Equals(OpenGenericTypeId? other) => Id == other?.Id;

    public override int GetHashCode() => Id.GetHashCode();
}

internal sealed record ClosedGenericTypeId : TypeId
{
    private static string GetId(Type type, IReadOnlyCollection<TypeId> args) =>
        $"{GetName(type)}<{string.Join(',', args.Select(x => x.Id))}>";

    public IReadOnlyCollection<TypeId> Arguments { get; }

    public ClosedGenericTypeId(
        Type type
    ) : base(
        type,
        type.GetGenericTypeDefinition(),
        GetName(type)
    )
    {
        Arguments = type.GetGenericArguments().Select(Create).ToArray();
        Id = GetId(type, Arguments);
    }

    public bool Equals(ClosedGenericTypeId? other) => Id == other?.Id;

    public override int GetHashCode() => Id.GetHashCode();
}