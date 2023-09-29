using System;

namespace Annium.Extensions.Reactive.Internal;

internal static class ObserverObservableEventExtensions
{
    public static void Handle<TSource>(this IObserver<TSource> observer, ObservableEvent<TSource> e)
    {
        if (e.Error is not null)
        {
            observer.OnError(e.Error);
            return;
        }

        if (e.IsCompleted)
        {
            observer.OnCompleted();
            return;
        }

        observer.OnNext(e.Data);
    }
}