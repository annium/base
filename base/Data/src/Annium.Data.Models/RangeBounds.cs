namespace Annium.Data.Models;

/// <summary>
/// Defines which boundaries of a range should be included in operations
/// </summary>
public enum RangeBounds
{
    /// <summary>
    /// Neither start nor end boundaries are included
    /// </summary>
    None,

    /// <summary>
    /// Only the start boundary is included
    /// </summary>
    Start,

    /// <summary>
    /// Only the end boundary is included
    /// </summary>
    End,

    /// <summary>
    /// Both start and end boundaries are included
    /// </summary>
    Both,
}
