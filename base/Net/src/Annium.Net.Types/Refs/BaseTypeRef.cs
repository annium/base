using Annium.Core.Runtime.Types;

namespace Annium.Net.Types.Refs;

[ResolutionKeyValue(RefType.BaseType)]
public sealed record BaseTypeRef(string Name) : IRef
{
    public RefType Type => RefType.BaseType;
}