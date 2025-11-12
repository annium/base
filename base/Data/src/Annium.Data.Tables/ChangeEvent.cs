using System;
using System.Collections.Generic;
using System.Linq;

namespace Annium.Data.Tables;

/// <summary>
/// Factory methods for creating change events that represent modifications to table data.
/// </summary>
public static class ChangeEvent
{
    /// <summary>
    /// Creates an initialization change event containing a collection of initial values.
    /// </summary>
    /// <typeparam name="T">The type of items in the change event.</typeparam>
    /// <param name="values">The initial collection of values to include in the event.</param>
    /// <returns>A change event representing table initialization.</returns>
    public static ChangeEvent<T> Init<T>(IReadOnlyCollection<T> values)
        where T : notnull
    {
        return new ChangeEvent<T>(values);
    }

    /// <summary>
    /// Creates a set change event for adding or updating a single item.
    /// </summary>
    /// <typeparam name="T">The type of item in the change event.</typeparam>
    /// <param name="value">The item to add or update.</param>
    /// <returns>A change event representing an item addition or update.</returns>
    public static ChangeEvent<T> Set<T>(T value)
        where T : notnull
    {
        return new ChangeEvent<T>(ChangeEventType.Set, value);
    }

    /// <summary>
    /// Creates a delete change event for removing a single item.
    /// </summary>
    /// <typeparam name="T">The type of item in the change event.</typeparam>
    /// <param name="value">The item to delete.</param>
    /// <returns>A change event representing an item deletion.</returns>
    public static ChangeEvent<T> Delete<T>(T value)
        where T : notnull
    {
        return new ChangeEvent<T>(ChangeEventType.Delete, value);
    }
}

/// <summary>
/// Represents a change event that describes modifications to table data, including initialization, item addition/update, and deletion.
/// </summary>
/// <typeparam name="T">The type of items affected by the change event.</typeparam>
public readonly struct ChangeEvent<T>
    where T : notnull
{
    /// <summary>
    /// Gets the type of change represented by this event.
    /// </summary>
    public readonly ChangeEventType Type;

    /// <summary>
    /// Gets the single item affected by this change event. Only valid for Set and Delete event types.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when accessed on an Init event type.</exception>
    public T Item
    {
        get
        {
            if (Type is not (ChangeEventType.Set or ChangeEventType.Delete))
                throw new InvalidOperationException($"this {typeof(T).FriendlyName()} is {Type}");

            return field!;
        }
    }

    /// <summary>
    /// Gets the collection of items for initialization events. Only valid for Init event type.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when accessed on Set or Delete event types.</exception>
    public IReadOnlyCollection<T> Items
    {
        get
        {
            if (Type is not ChangeEventType.Init)
                throw new InvalidOperationException($"this {typeof(T).FriendlyName()} is {Type}");

            return field!;
        }
    }

    /// <summary>
    /// Initializes a new change event for a single item operation (Set or Delete).
    /// </summary>
    /// <param name="type">The type of change event.</param>
    /// <param name="item">The item affected by the change.</param>
    public ChangeEvent(ChangeEventType type, T item)
    {
        Type = type;
        Item = item;
    }

    /// <summary>
    /// Initializes a new change event for table initialization.
    /// </summary>
    /// <param name="items">The initial collection of items.</param>
    internal ChangeEvent(IReadOnlyCollection<T> items)
    {
        Type = ChangeEventType.Init;
        Items = items;
    }

    /// <summary>
    /// Determines whether this change event is equal to another change event.
    /// </summary>
    /// <param name="other">The other change event to compare with.</param>
    /// <returns>True if the events are equal; otherwise, false.</returns>
    public bool Equals(ChangeEvent<T>? other)
    {
        if (other is null || Type != other.Value.Type)
            return false;

        if (Type is ChangeEventType.Init)
            return Items.SequenceEqual(other.Value.Items);

        return Item.Equals(other.Value.Item);
    }

    /// <summary>
    /// Returns a string representation of this change event.
    /// </summary>
    /// <returns>A string describing the change event type and affected items.</returns>
    public override string ToString()
    {
        if (Type is ChangeEventType.Init)
            return $"{Type}: {typeof(T).FriendlyName()}[{Items.Count}]";

        return $"{Type}: {typeof(T).FriendlyName()}";
    }
}
