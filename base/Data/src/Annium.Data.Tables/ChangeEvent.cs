using System;
using System.Collections.Generic;
using System.Linq;

namespace Annium.Data.Tables;

public static class ChangeEvent
{
    public static ChangeEvent<T> Init<T>(IReadOnlyCollection<T> values)
        where T : notnull
    {
        return new ChangeEvent<T>(values);
    }

    public static ChangeEvent<T> Set<T>(T value)
        where T : notnull
    {
        return new ChangeEvent<T>(ChangeEventType.Set, value);
    }

    public static ChangeEvent<T> Delete<T>(T value)
        where T : notnull
    {
        return new ChangeEvent<T>(ChangeEventType.Delete, value);
    }
}

public readonly struct ChangeEvent<T>
    where T : notnull
{
    public readonly ChangeEventType Type;
    public T Item
    {
        get
        {
            if (Type is not (ChangeEventType.Set or ChangeEventType.Delete))
                throw new InvalidOperationException($"this {typeof(T).FriendlyName()} is {Type}");

            return _item!;
        }
    }

    public IReadOnlyCollection<T> Items
    {
        get
        {
            if (Type is not ChangeEventType.Init)
                throw new InvalidOperationException($"this {typeof(T).FriendlyName()} is {Type}");

            return _items!;
        }
    }

    private readonly T? _item;
    private readonly IReadOnlyCollection<T>? _items;

    public ChangeEvent(ChangeEventType type, T item)
    {
        Type = type;
        _item = item;
    }

    internal ChangeEvent(IReadOnlyCollection<T> items)
    {
        Type = ChangeEventType.Init;
        _items = items;
    }

    public bool Equals(ChangeEvent<T>? other)
    {
        if (other is null || Type != other.Value.Type)
            return false;

        if (Type is ChangeEventType.Init)
            return Items.SequenceEqual(other.Value.Items);

        return Item.Equals(other.Value.Item);
    }

    public override string ToString()
    {
        if (Type is ChangeEventType.Init)
            return $"{Type}: {typeof(T).FriendlyName()}[{Items.Count}]";

        return $"{Type}: {typeof(T).FriendlyName()}";
    }
}
