namespace Annium.Extensions.Pooling;

/// <summary>
/// Defines what a pool reaches for first when handing an instance out. Both modes create on demand -
/// nothing is built when the pool is constructed - and both stop creating at the configured capacity.
/// </summary>
public enum PoolLoadMode
{
    /// <summary>
    /// A new instance is created while the pool is below capacity, in preference to reusing a free one,
    /// so the pool fills up to capacity as it is used.
    /// </summary>
    Eager,

    /// <summary>
    /// A free instance is reused when there is one, and a new one is created only when there is not,
    /// so the pool grows no larger than concurrent demand requires.
    /// </summary>
    Lazy,
}
