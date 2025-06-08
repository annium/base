using Annium.Internal;

namespace Annium;

/// <summary>
/// Provides a factory for creating tracking weak references to objects.
/// </summary>
public sealed class TrackingWeakReference
{
    /// <summary>
    /// Gets a tracking weak reference for the specified target object.
    /// </summary>
    /// <typeparam name="T">The type of the target object.</typeparam>
    /// <param name="target">The target object to track.</param>
    /// <returns>A tracking weak reference for the target object.</returns>
    public static ITrackingWeakReference<T> Get<T>(T target)
        where T : class
    {
        return TrackingWeakReference<T>.Registry.GetValue(target, key => new TrackingWeakReference<T>(key));
    }
}
