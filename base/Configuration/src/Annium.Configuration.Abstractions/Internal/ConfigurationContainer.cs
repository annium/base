using System.Collections.Generic;

namespace Annium.Configuration.Abstractions.Internal;

/// <summary>
/// Implementation of IConfigurationContainer that stores configuration data in memory
/// </summary>
public class ConfigurationContainer : IConfigurationContainer
{
    /// <summary>
    /// Dictionary storing configuration data with custom key comparer
    /// </summary>
    private readonly IDictionary<string[], string> _config = new Dictionary<string[], string>(new KeyComparer());

    /// <summary>
    /// Adds configuration data to the container
    /// </summary>
    /// <param name="config">Configuration data to add</param>
    /// <returns>The container for method chaining</returns>
    public IConfigurationContainer Add(IReadOnlyDictionary<string[], string> config)
    {
        foreach (var (key, value) in config)
            _config[key] = value;

        return this;
    }

    /// <summary>
    /// Gets all configuration data from the container
    /// </summary>
    /// <returns>Dictionary containing all configuration data</returns>
    public IReadOnlyDictionary<string[], string> Get() => new Dictionary<string[], string>(_config, new KeyComparer());

    /// <summary>
    /// Custom equality comparer for string arrays that compares using camel case
    /// </summary>
    private class KeyComparer : IEqualityComparer<string[]>
    {
        /// <summary>
        /// Determines whether two string arrays are equal using case-insensitive comparison
        /// </summary>
        /// <param name="x">First string array</param>
        /// <param name="y">Second string array</param>
        /// <returns>True if arrays are equal, false otherwise</returns>
        public bool Equals(string[]? x, string[]? y)
        {
            // if same reference or both null, then equality is true
            if (ReferenceEquals(x, y))
                return true;

            // if any is null, or length doesn't match - false
            if (x == null || y == null || x.Length != y.Length)
                return false;

            // check, that all elements are equal case independently
            for (var i = 0; i < x.Length; i++)
                if (x[i].CamelCase() != y[i].CamelCase())
                    return false;

            // if no mismatches, equal
            return true;
        }

        /// <summary>
        /// Returns a hash code for the string array using camel case conversion
        /// </summary>
        /// <param name="obj">String array to get hash code for</param>
        /// <returns>Hash code for the array</returns>
        public int GetHashCode(string[] obj)
        {
            unchecked
            {
                var hash = 17;

                // get hash code for all items in array
                foreach (var item in obj)
                {
                    hash = hash * 23 + item.CamelCase().GetHashCode();
                }

                return hash;
            }
        }
    }
}
