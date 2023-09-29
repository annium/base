using System.Collections.Generic;

namespace Annium.Net.Types.Models;

public sealed record EnumModel(
    Namespace Namespace,
    string Name,
    IReadOnlyDictionary<string, long> Values
) : IModel
{
    public override string ToString() => $"enum {Namespace}.{Name}";
}