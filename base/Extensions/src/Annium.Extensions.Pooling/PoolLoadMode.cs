namespace Annium.Extensions.Pooling;

/// <summary>
/// Defines when a pool creates the instances it hands out.
/// </summary>
public enum PoolLoadMode
{
    /// <summary>
    /// All instances are created up front, when the pool is constructed.
    /// </summary>
    Eager,

    /// <summary>
    /// Instances are created on demand, as capacity is claimed for the first time.
    /// </summary>
    Lazy,
}
