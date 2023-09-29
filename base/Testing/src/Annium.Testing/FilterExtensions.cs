using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Testing.Elements;

namespace Annium.Testing;

public static class FilterExtensions
{
    public static IEnumerable<Test> FilterMask(this IEnumerable<Test> tests, string mask)
    {
        if (string.IsNullOrWhiteSpace(mask))
            return tests;

        var list = tests.ToList();
        var comparison = StringComparison.CurrentCultureIgnoreCase;

        return list.Where(t => t.DisplayName.Contains(mask, comparison)).ToArray();
    }
}