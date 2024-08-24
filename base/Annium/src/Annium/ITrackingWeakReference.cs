using System;

namespace Annium;

public interface ITrackingWeakReference<T>
    where T : class
{
    event Action OnCollected;
    bool IsAlive { get; }
    public bool TryGetTarget(out T target);
}
