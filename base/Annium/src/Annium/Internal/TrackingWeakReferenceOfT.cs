using System;
using System.Runtime.CompilerServices;

namespace Annium.Internal;

internal sealed class TrackingWeakReference<T> : ITrackingWeakReference<T>
    where T : class
{
    public static readonly ConditionalWeakTable<T, TrackingWeakReference<T>> Registry = new();

    public event Action Collected = delegate { };
    private readonly WeakReference<T> _ref;

    public bool IsAlive => TryGetTarget(out _);

    public TrackingWeakReference(T target)
    {
        _ref = new WeakReference<T>(target);
    }

    public bool TryGetTarget(out T target) => _ref.TryGetTarget(out target!);

    ~TrackingWeakReference()
    {
        Collected.Invoke();
    }
}