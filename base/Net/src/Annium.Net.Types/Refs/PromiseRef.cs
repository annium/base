using Annium.Core.Runtime.Types;

namespace Annium.Net.Types.Refs;

[ResolutionKeyValue(RefType.Promise)]
public sealed record PromiseRef(IRef? Value) : IRef
{
    public RefType Type => RefType.Promise;
}