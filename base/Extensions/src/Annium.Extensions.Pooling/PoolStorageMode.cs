namespace Annium.Extensions.Pooling;

/// <summary>
/// Defines the order in which a pool hands out the instances it holds.
/// </summary>
public enum PoolStorageMode
{
    /// <summary>
    /// First in, first out — the instance returned longest ago is claimed next.
    /// </summary>
    Fifo,

    /// <summary>
    /// Last in, first out — the most recently returned instance is claimed next.
    /// </summary>
    Lifo,
}
