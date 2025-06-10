using System;
using System.Reactive;
using System.Reactive.Linq;

namespace Annium.Extensions.Reactive.Operators;

/// <summary>
/// Provides operators for converting observables to unit-type observables
/// </summary>
public static class UnitOperatorExtensions
{
    /// <summary>
    /// Converts an observable sequence to emit Unit values instead of the original values, preserving the timing and completion behavior
    /// </summary>
    /// <typeparam name="TSource">The type of items emitted by the source observable</typeparam>
    /// <param name="source">The source observable to convert</param>
    /// <returns>An observable that emits Unit.Default for each value from the source</returns>
    public static IObservable<Unit> Unit<TSource>(this IObservable<TSource> source) =>
        source.Select(_ => System.Reactive.Unit.Default);
}
