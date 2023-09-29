using Annium.Internal;

namespace Annium;

public sealed class TrackingWeakReference
{
    public static ITrackingWeakReference<T> Get<T>(T target)
        where T : class
    {
        return TrackingWeakReference<T>.Registry.GetValue(target, key => new TrackingWeakReference<T>(key));
    }
}