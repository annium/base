using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Core.DependencyInjection.Internal.Builders.Registrations;
using Annium.Core.DependencyInjection.Internal.Container;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class IndexExtensions
{
    public static IIndex<TKey, TElement> ToIndex<TSource, TKey, TElement>(
        this IEnumerable<TSource> src,
        Func<TSource, TKey> keySelector,
        Func<TSource, TElement> elementSelector
    )
        where TKey : notnull
        where TElement : notnull
    {
        return new Index<TKey, TElement>(src.Select(x => new KeyValue<TKey, TElement>(keySelector(x), () => elementSelector(x))));
    }
}