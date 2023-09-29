using System;
using System.Runtime.CompilerServices;
using Annium.Testing;
using Xunit;

namespace Annium.Tests;

public class TrackingWeakReferenceTest
{
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
            reference.Collected += () => counter++;
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

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void Wrap(Action wrap) => wrap();
}