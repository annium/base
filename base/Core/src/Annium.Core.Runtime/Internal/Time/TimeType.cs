namespace Annium.Core.Runtime.Internal.Time;

/// <summary>
/// Internal enumeration-like class representing different time provider types
/// </summary>
internal sealed class TimeType
{
    /// <summary>
    /// Real system time provider type
    /// </summary>
    public static readonly TimeType Real = new(nameof(Real));

    /// <summary>
    /// Relative time provider type
    /// </summary>
    public static readonly TimeType Relative = new(nameof(Relative));

    /// <summary>
    /// Managed time provider type
    /// </summary>
    public static readonly TimeType Managed = new(nameof(Managed));

    /// <summary>
    /// The name of the time type
    /// </summary>
    private readonly string _name;

    /// <summary>
    /// Initializes a new instance of TimeType with the specified name
    /// </summary>
    /// <param name="name">The name of the time type</param>
    private TimeType(string name)
    {
        _name = name;
    }

    /// <summary>
    /// Returns the string representation of the time type
    /// </summary>
    /// <returns>The name of the time type</returns>
    public override string ToString() => _name;
}
