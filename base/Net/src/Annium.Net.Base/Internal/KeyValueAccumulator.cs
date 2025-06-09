using System;
using System.Collections.Generic;
using Microsoft.Extensions.Primitives;

namespace Annium.Net.Base.Internal;

/// <summary>
/// Accumulates key-value pairs for efficient string value collection
/// </summary>
internal struct KeyValueAccumulator
{
    /// <summary>
    /// Primary dictionary for storing key-value pairs with up to 2 values per key
    /// </summary>
    private Dictionary<string, StringValues> _accumulator;

    /// <summary>
    /// Secondary dictionary for storing keys with 3 or more values
    /// </summary>
    private Dictionary<string, List<string>> _expandingAccumulator;

    /// <summary>
    /// Appends a key-value pair to the accumulator
    /// </summary>
    /// <param name="key">The key to append</param>
    /// <param name="value">The value to append</param>
    public void Append(string key, string value)
    {
        _accumulator ??= new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase);

        if (!_accumulator.TryGetValue(key, out var values))
        {
            // First value for this key
            _accumulator[key] = new StringValues(value);
        }
        else
            switch (values.Count)
            {
                case 0:
                    // Marker entry for this key to indicate entry already in expanding list dictionary
                    _expandingAccumulator[key].Add(value);
                    break;
                case 1:
                    // Second value for this key
                    _accumulator[key] = new[] { values[0], value };
                    break;
                default:
                {
                    // Third value for this key
                    // Add zero count entry and move to data to expanding list dictionary
                    _accumulator[key] = default;

                    _expandingAccumulator ??= new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

                    // Already 3 entries so use starting allocated as 8; then use List's expansion mechanism for more
                    var list = new List<string>(8);
                    var array = values.ToArray();

                    list.Add(array?[0] ?? string.Empty);
                    list.Add(array?[1] ?? string.Empty);
                    list.Add(value);

                    _expandingAccumulator[key] = list;
                    break;
                }
            }
    }

    /// <summary>
    /// Gets the accumulated results as a dictionary
    /// </summary>
    /// <returns>A dictionary containing all accumulated key-value pairs</returns>
    public Dictionary<string, StringValues> GetResults()
    {
        if (_expandingAccumulator is null)
            return _accumulator ?? new Dictionary<string, StringValues>(0, StringComparer.OrdinalIgnoreCase);

        // Coalesce count 3+ multi-value entries into _accumulator dictionary
        foreach (var entry in _expandingAccumulator)
        {
            _accumulator[entry.Key] = new StringValues(entry.Value.ToArray());
        }

        return _accumulator ?? new Dictionary<string, StringValues>(0, StringComparer.OrdinalIgnoreCase);
    }
}
