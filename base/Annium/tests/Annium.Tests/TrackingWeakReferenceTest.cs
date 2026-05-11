using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Annium.Testing;
using Xunit;

namespace Annium.Tests;

/// <summary>
/// Contains unit tests for the TrackingWeakReference class.
/// </summary>
public class TrackingWeakReferenceTest
{
    /// <summary>
    /// Verifies that TrackingWeakReference correctly tracks object collection and raises the OnCollected event.
    /// The event fires off the finalizer thread (queued to the ThreadPool), so we wait for it via a signal.
    /// </summary>
    [Fact]
    public void TrackingWeakReference_Works()
    {
        // arrange
        using var collected = new ManualResetEventSlim(initialState: false);
        var counter = 0;
        object target;
        ITrackingWeakReference<object> reference = default!;
        Wrap(() =>
        {
            target = new object();
            reference = TrackingWeakReference.Get(target);
            reference.OnCollected += () =>
            {
                Interlocked.Increment(ref counter);
                collected.Set();
            };
        });

        // act
        target = default!;
        GC.Collect();
        GC.WaitForPendingFinalizers();

        // assert
        reference.IsAlive.IsFalse();

        // act
        reference = default!;
        GC.Collect();
        GC.WaitForPendingFinalizers();

        // assert - OnCollected runs on the ThreadPool, so wait for the signal before reading the counter.
        collected.Wait(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken).IsTrue();
        counter.Is(1);
    }

    /// <summary>
    /// Wraps an action to prevent inlining, ensuring proper garbage collection behavior.
    /// </summary>
    /// <param name="wrap">The action to wrap.</param>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void Wrap(Action wrap) => wrap();
}
