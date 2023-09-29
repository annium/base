using System;

namespace Annium.Core.Mediator.Internal;

internal record Match
{
    public Type RequestedType { get; }
    public Type ExpectedType { get; }
    public Type ResolvedType { get; }

    internal Match(
        Type requestedType,
        Type expectedType,
        Type resolvedType
    )
    {
        RequestedType = requestedType;
        ExpectedType = expectedType;
        ResolvedType = resolvedType;
    }

    public override string ToString() => $"{RequestedType.FriendlyName()} -> {ResolvedType.FriendlyName()} ({ExpectedType.FriendlyName()})";
}