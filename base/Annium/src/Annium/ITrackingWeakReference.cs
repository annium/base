using System;

namespace Annium;

public interface ITrackingWeakReference<T>
    where T : class
{
    event Action OnCollected;
    bool IsAlive { get; }
    bool TryGetTarget(out T target);
}
