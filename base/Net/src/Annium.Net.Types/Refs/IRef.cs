using Annium.Core.Runtime.Types;

namespace Annium.Net.Types.Refs;

public interface IRef
{
    [ResolutionKey]
    RefType Type { get; }
}
