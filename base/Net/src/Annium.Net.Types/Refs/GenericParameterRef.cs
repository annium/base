using Annium.Core.Runtime.Types;

namespace Annium.Net.Types.Refs;

[ResolutionKeyValue(RefType.GenericParameter)]
public sealed record GenericParameterRef(string Name) : IRef
{
    public RefType Type => RefType.GenericParameter;
}