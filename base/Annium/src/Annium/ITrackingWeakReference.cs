using System;

namespace Annium;

public interface ITrackingWeakReference<T>
    where T : class
{
    event Action Collected;
    bool IsAlive { get; }
    public bool TryGetTarget(out T target);
}