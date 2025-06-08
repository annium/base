using System;
using System.Runtime.CompilerServices;
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
    /// </summary>
    [Fact]
    public void TrackingWeakReference_Works()
    {
        // arrange
        var counter = 0;
        object target;
        ITrackingWeakReference<object> reference = default!;
        Wrap(() =>
        {
            target = new object();
            reference = TrackingWeakReference.Get(target);
            reference.OnCollected += () => counter++;
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

        // assert
        counter.Is(1);
    }

    /// <summary>
    /// Wraps an action to prevent inlining, ensuring proper garbage collection behavior.
    /// </summary>
    /// <param name="wrap">The action to wrap.</param>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void Wrap(Action wrap) => wrap();
}
