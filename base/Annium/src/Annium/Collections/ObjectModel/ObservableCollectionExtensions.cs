using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Annium.Collections.ObjectModel;

public static class ObservableCollectionExtensions
{
    public static void Sort<T>(this ObservableCollection<T> collection, Comparison<T> comparison)
    {
        var sortableList = new List<T>(collection);
        sortableList.Sort(comparison);

        for (var i = 0; i < sortableList.Count; i++)
            collection.Move(collection.IndexOf(sortableList[i]), i);
    }

    public static void ForceSort<T>(this ObservableCollection<T> collection, Comparison<T> comparison)
    {
        var sortableList = new List<T>(collection);
        sortableList.Sort(comparison);

        collection.Clear();
        foreach (var item in sortableList)
            collection.Add(item);
    }
}
