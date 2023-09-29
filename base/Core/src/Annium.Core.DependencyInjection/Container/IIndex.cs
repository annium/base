using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public interface IIndex<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
    where TKey : notnull
    where TValue : notnull
{
}