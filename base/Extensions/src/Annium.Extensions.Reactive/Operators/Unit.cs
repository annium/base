using System.Reactive;
using System.Reactive.Linq;

// ReSharper disable once CheckNamespace
namespace System;

public static class UnitOperatorExtensions
{
    public static IObservable<Unit> Unit<TSource>(
        this IObservable<TSource> source
    ) => source.Select(_ => Reactive.Unit.Default);
}