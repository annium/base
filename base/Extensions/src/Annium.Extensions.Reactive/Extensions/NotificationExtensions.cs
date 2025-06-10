using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Linq;
using Annium.Reflection;

namespace Annium.Extensions.Reactive.Extensions;

/// <summary>
/// Provides extension methods for creating observables from property change notifications
/// </summary>
public static class NotificationExtensions
{
    /// <summary>
    /// Creates an observable that emits a unit value whenever any property changes on the target object
    /// </summary>
    /// <param name="target">The object implementing INotifyPropertyChanged to observe</param>
    /// <returns>An observable that emits Unit.Default when any property changes</returns>
    public static IObservable<Unit> WhenAnyPropertyChanges(this INotifyPropertyChanged target)
    {
        return Observable
            .FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                h => target.PropertyChanged += h,
                h => target.PropertyChanged -= h
            )
            .Select(_ => Unit.Default);
    }

    /// <summary>
    /// Creates an observable that emits a unit value when a specific property changes on the target object
    /// </summary>
    /// <typeparam name="TTarget">The type of the target object</typeparam>
    /// <typeparam name="TProperty">The type of the property to observe</typeparam>
    /// <param name="target">The object implementing INotifyPropertyChanged to observe</param>
    /// <param name="property">An expression identifying the property to observe</param>
    /// <returns>An observable that emits Unit.Default when the specified property changes</returns>
    public static IObservable<Unit> WhenPropertyChanges<TTarget, TProperty>(
        this TTarget target,
        Expression<Func<TTarget, TProperty>> property
    )
        where TTarget : INotifyPropertyChanged
    {
        var propertyName = TypeHelper.ResolveProperty(property).Name;

        return Observable
            .FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                h => target.PropertyChanged += h,
                h => target.PropertyChanged -= h
            )
            .Where(x => x.EventArgs.PropertyName == propertyName)
            .Select(_ => Unit.Default);
    }

    /// <summary>
    /// Creates an observable that emits the current value of a specific property when it changes on the target object
    /// </summary>
    /// <typeparam name="TTarget">The type of the target object</typeparam>
    /// <typeparam name="TProperty">The type of the property to observe</typeparam>
    /// <param name="target">The object implementing INotifyPropertyChanged to observe</param>
    /// <param name="property">An expression identifying the property to observe</param>
    /// <returns>An observable that emits the current property value when the specified property changes</returns>
    public static IObservable<TProperty> GetPropertyChanges<TTarget, TProperty>(
        this TTarget target,
        Expression<Func<TTarget, TProperty>> property
    )
        where TTarget : INotifyPropertyChanged
    {
        var propertyName = TypeHelper.ResolveProperty(property).Name;
        var getProperty = property.Compile();

        return Observable
            .FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                h => target.PropertyChanged += h,
                h => target.PropertyChanged -= h
            )
            .Where(x => x.EventArgs.PropertyName == propertyName)
            .Select(_ => getProperty(target));
    }
}
