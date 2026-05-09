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
    /// <remarks>
    /// The returned reference is shared per (T, target) pair via a backing
    /// <see cref="System.Runtime.CompilerServices.ConditionalWeakTable{TKey,TValue}"/>: every call with the same
    /// target returns the SAME instance, and therefore subscribers attached to its <c>OnCollected</c> event are
    /// shared across all callers. This is intentional so that GC notifications are delivered exactly once per
    /// collected target, but it means subscribers may observe events triggered by code they did not initiate.
    /// Callers MUST NOT rely on subscriber isolation; use a wrapper if independent event semantics are required.
    /// </remarks>
    /// <typeparam name="T">The type of the target object.</typeparam>
    /// <param name="target">The target object to track.</param>
    /// <returns>A shared tracking weak reference for the target object.</returns>
    public static ITrackingWeakReference<T> Get<T>(T target)
        where T : class
    {
        return TrackingWeakReference<T>.Registry.GetValue(target, key => new TrackingWeakReference<T>(key));
    }
}
