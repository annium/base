namespace Annium.Linq;

/// <summary>
/// Enum defining merge behavior for merging two collections
/// </summary>
public enum MergeBehavior
{
    /// <summary>
    /// Marks that source values are preferred, target values are discarded on conflict
    /// </summary>
    KeepSource,

    /// <summary>
    /// Marks that target values are preferred, source values are discarded on conflict
    /// </summary>
    KeepTarget,
}
