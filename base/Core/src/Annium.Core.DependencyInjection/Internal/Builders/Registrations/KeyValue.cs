using System;

namespace Annium.Core.DependencyInjection.Internal.Builders.Registrations;

public sealed record KeyValue<TKey, TValue>
{
    public TKey Key { get; }
    public Lazy<TValue> Value { get; }

    public KeyValue(TKey key, Func<TValue> getValue)
    {
        Key = key;
        Value = new Lazy<TValue>(getValue, true);
    }
}