using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Annium.Collections.ObjectModel;

/// <summary>
/// Provides extension methods for working with <see cref="ObservableCollection{T}"/>.
/// </summary>
public static class ObservableCollectionExtensions
{
    /// <summary>
    /// Sorts the elements of the collection using the specified comparison.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="collection">The collection to sort.</param>
    /// <param name="comparison">The comparison to use when sorting elements.</param>
    public static void Sort<T>(this ObservableCollection<T> collection, Comparison<T> comparison)
    {
        var sortableList = new List<T>(collection);
        sortableList.Sort(comparison);

        for (var i = 0; i < sortableList.Count; i++)
            collection.Move(collection.IndexOf(sortableList[i]), i);
    }

    /// <summary>
    /// Forces a sort of the collection by clearing and re-adding all elements in sorted order.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="collection">The collection to sort.</param>
    /// <param name="comparison">The comparison to use when sorting elements.</param>
    public static void ForceSort<T>(this ObservableCollection<T> collection, Comparison<T> comparison)
    {
        var sortableList = new List<T>(collection);
        sortableList.Sort(comparison);

        collection.Clear();
        foreach (var item in sortableList)
            collection.Add(item);
    }
}
