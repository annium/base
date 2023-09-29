using System.ComponentModel;
using System.Linq.Expressions;
using Annium.Reflection;

// ReSharper disable once CheckNamespace
namespace System.Reactive.Linq;

public static class NotificationExtensions
{
    public static IObservable<Unit> WhenAnyPropertyChanges(
        this INotifyPropertyChanged target
    )
    {
        return Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                h => target.PropertyChanged += h,
                h => target.PropertyChanged -= h
            )
            .Select(_ => Unit.Default);
    }

    public static IObservable<Unit> WhenPropertyChanges<TTarget, TProperty>(
        this TTarget target,
        Expression<Func<TTarget, TProperty>> property
    )
        where TTarget : INotifyPropertyChanged
    {
        var propertyName = TypeHelper.ResolveProperty(property).Name;

        return Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                h => target.PropertyChanged += h,
                h => target.PropertyChanged -= h
            )
            .Where(x => x.EventArgs.PropertyName == propertyName)
            .Select(_ => Unit.Default);
    }

    public static IObservable<TProperty> GetPropertyChanges<TTarget, TProperty>(
        this TTarget target,
        Expression<Func<TTarget, TProperty>> property
    )
        where TTarget : INotifyPropertyChanged
    {
        var propertyName = TypeHelper.ResolveProperty(property).Name;
        var getProperty = property.Compile();

        return Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                h => target.PropertyChanged += h,
                h => target.PropertyChanged -= h
            )
            .Where(x => x.EventArgs.PropertyName == propertyName)
            .Select(_ => getProperty(target));
    }
}