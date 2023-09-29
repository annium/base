using System;
using Annium.Core.Runtime.Types;

namespace Annium.Net.Types.Refs;

[ResolutionKeyValue(RefType.Interface)]
public sealed record InterfaceRef(string Namespace, string Name, params IRef[] Args) : IGenericModelRef
{
    public RefType Type => RefType.Interface;
    public override int GetHashCode() => HashCode.Combine(Namespace, Name, HashCodeSeq.Combine(Args));
    public bool Equals(InterfaceRef? other) => GetHashCode() == other?.GetHashCode();
}