using Annium.Net.Types.Refs;

namespace Annium.Net.Types.Models;

public sealed record FieldModel(
    IRef Type,
    string Name
)
{
    public override string ToString() => $"{Type} {Name}";
}