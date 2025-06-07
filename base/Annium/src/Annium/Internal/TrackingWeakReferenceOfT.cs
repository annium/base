using System;
using System.Runtime.CompilerServices;

namespace Annium.Internal;

/// <summary>
/// Provides a weak reference to an object that can be tracked for garbage collection.
/// </summary>
/// <typeparam name="T">The type of the object to reference.</typeparam>
internal sealed class TrackingWeakReference<T> : ITrackingWeakReference<T>
    where T : class
{
    /// <summary>
    /// A registry that maintains the association between target objects and their tracking references.
    /// </summary>
    public static readonly ConditionalWeakTable<T, TrackingWeakReference<T>> Registry = new();

    /// <summary>
    /// Occurs when the referenced object is collected by the garbage collector.
    /// </summary>
    public event Action OnCollected = delegate { };

    /// <summary>
    /// The underlying weak reference to the target object.
    /// </summary>
    private readonly WeakReference<T> _ref;

    /// <summary>
    /// Gets a value indicating whether the referenced object is still alive.
    /// </summary>
    public bool IsAlive => TryGetTarget(out _);

    /// <summary>
    /// Initializes a new instance of the <see cref="TrackingWeakReference{T}"/> class.
    /// </summary>
    /// <param name="target">The object to track.</param>
    public TrackingWeakReference(T target)
    {
        _ref = new WeakReference<T>(target);
    }

    /// <summary>
    /// Attempts to get the referenced object.
    /// </summary>
    /// <param name="target">When this method returns, contains the referenced object if it is still alive; otherwise, null.</param>
    /// <returns>true if the referenced object is still alive; otherwise, false.</returns>
    public bool TryGetTarget(out T target) => _ref.TryGetTarget(out target!);

    /// <summary>
    /// Finalizes the instance and raises the <see cref="OnCollected"/> event.
    /// </summary>
    ~TrackingWeakReference()
    {
        OnCollected.Invoke();
    }
}
