using System;

namespace Annium;

/// <summary>
/// Represents a weak reference that tracks when its target object is collected by the garbage collector.
/// </summary>
/// <typeparam name="T">The type of the target object.</typeparam>
public interface ITrackingWeakReference<T>
    where T : class
{
    /// <summary>
    /// Occurs when the target object is collected by the garbage collector.
    /// </summary>
    event Action OnCollected;

    /// <summary>
    /// Gets a value indicating whether the target object is still alive.
    /// </summary>
    bool IsAlive { get; }

    /// <summary>
    /// Attempts to get the target object.
    /// </summary>
    /// <param name="target">When this method returns, contains the target object if it is still alive; otherwise, null.</param>
    /// <returns>true if the target object is still alive; otherwise, false.</returns>
    bool TryGetTarget(out T target);
}
