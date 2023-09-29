using System.Collections.Generic;

namespace Annium.Configuration.Abstractions.Internal;

public class ConfigurationContainer : IConfigurationContainer
{
    private readonly IDictionary<string[], string> _config = new Dictionary<string[], string>(new KeyComparer());

    public IConfigurationContainer Add(IReadOnlyDictionary<string[], string> config)
    {
        foreach (var (key, value) in config)
            _config[key] = value;

        return this;
    }

    public IReadOnlyDictionary<string[], string> Get() => new Dictionary<string[], string>(_config, new KeyComparer());

    private class KeyComparer : IEqualityComparer<string[]>
    {
        public bool Equals(string[]? x, string[]? y)
        {
            // if same reference or both null, then equality is true
            if (ReferenceEquals(x, y))
                return true;

            // if any is null, or length doesn't match - false
            if (x == null || y == null || x.Length != y.Length)
                return false;

            // check, that all elements are equal case independently
            for (int i = 0; i < x.Length; i++)
                if (x[i].CamelCase() != y[i].CamelCase())
                    return false;

            // if no mismatches, equal
            return true;
        }

        public int GetHashCode(string[] obj)
        {
            unchecked
            {
                int hash = 17;

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