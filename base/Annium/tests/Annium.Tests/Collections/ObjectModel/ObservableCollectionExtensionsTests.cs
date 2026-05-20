using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Annium.Collections.ObjectModel;
using Annium.Testing;
using Xunit;

namespace Annium.Tests.Collections.ObjectModel;

/// <summary>
/// Contains unit tests for <see cref="ObservableCollectionExtensions"/> to verify sorting behavior.
/// </summary>
public class ObservableCollectionExtensionsTests
{
    /// <summary>
    /// Verifies that the Sort extension method correctly sorts an ObservableCollection.
    /// </summary>
    [Fact]
    public void Sort()
    {
        var data = new ObservableCollection<int>();
        var maxValue = 10;

        // Populate the list in reverse mode [maxValue, maxValue-1, ..., 1, 0]
        for (var i = maxValue; i >= 0; i--)
        {
            data.Add(i);
        }

        // Assert the collection is in reverse mode
        for (var i = maxValue; i >= 0; i--)
            data[maxValue - i].Is(i);

        // Sort the observable collection
        data.Sort((a, b) => a.CompareTo(b));

        // Assert elements have been sorted
        for (var i = 0; i < maxValue; i++)
            data[i].Is(i);
    }

    /// <summary>
    /// Verifies that ForceSort places elements in ascending order, including duplicate values.
    /// </summary>
    [Fact]
    public void ForceSort_UnsortedCollection_ItemsInSortedOrder()
    {
        // arrange
        var coll = new ObservableCollection<int> { 3, 1, 4, 1, 5 };

        // act
        coll.ForceSort(Comparer<int>.Default.Compare);

        // assert
        coll.SequenceEqual(new[] { 1, 1, 3, 4, 5 }).IsTrue();
    }

    /// <summary>
    /// Verifies that ForceSort on an empty collection leaves it empty.
    /// </summary>
    [Fact]
    public void ForceSort_EmptyCollection_RemainsEmpty()
    {
        // arrange
        var coll = new ObservableCollection<int>();

        // act
        coll.ForceSort(Comparer<int>.Default.Compare);

        // assert
        coll.IsEmpty();
        coll.Has(0);
    }
}
