using System.Collections.ObjectModel;
using Annium.Collections.ObjectModel;
using Annium.Testing;
using Xunit;

namespace Annium.Tests.Collections.ObjectModel;

public class ObservableCollectionExtensionsTests
{
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
}
