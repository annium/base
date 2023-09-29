namespace Annium.Core.Runtime.Internal.Time;

internal sealed class TimeType
{
    public static readonly TimeType Real = new(nameof(Real));
    public static readonly TimeType Relative = new(nameof(Relative));
    public static readonly TimeType Managed = new(nameof(Managed));

    private readonly string _name;

    private TimeType(string name)
    {
        _name = name;
    }

    public override string ToString() => _name;
}