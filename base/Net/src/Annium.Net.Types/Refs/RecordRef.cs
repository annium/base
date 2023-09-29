using Annium.Core.Runtime.Types;

namespace Annium.Net.Types.Refs;

[ResolutionKeyValue(RefType.Record)]
public sealed record RecordRef(IRef Key, IRef Value) : IRef
{
    public RefType Type => RefType.Record;
}