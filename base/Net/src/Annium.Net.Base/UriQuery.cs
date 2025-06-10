using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Annium.Net.Base.Internal;
using Microsoft.Extensions.Primitives;

namespace Annium.Net.Base;

/// <summary>
/// Represents a URI query string with key-value parameters
/// </summary>
public sealed record UriQuery
    : IDictionary<string, StringValues>,
        IReadOnlyDictionary<string, StringValues>,
        ICopyable<UriQuery>
{
    /// <summary>
    /// Creates a new empty UriQuery instance
    /// </summary>
    /// <returns>A new UriQuery instance</returns>
    public static UriQuery New()
    {
        return new(new Dictionary<string, StringValues>());
    }

    /// <summary>
    /// Parses a query string into a UriQuery instance
    /// </summary>
    /// <param name="query">The query string to parse</param>
    /// <returns>A new UriQuery instance containing the parsed parameters</returns>
    public static UriQuery Parse(string query)
    {
        var data = QueryHelpers.ParseQuery(query);

        return new UriQuery(data);
    }

    /// <summary>
    /// The internal dictionary storing the query parameters
    /// </summary>
    private readonly IDictionary<string, StringValues> _data;

    private UriQuery(IDictionary<string, StringValues> data)
    {
        _data = data;
    }

    /// <summary>
    /// Returns an enumerator that iterates through the query parameters
    /// </summary>
    /// <returns>An enumerator for the query parameters</returns>
    public IEnumerator<KeyValuePair<string, StringValues>> GetEnumerator() => _data.GetEnumerator();

    /// <summary>
    /// Returns an enumerator that iterates through the query parameters
    /// </summary>
    /// <returns>An enumerator for the query parameters</returns>
    IEnumerator IEnumerable.GetEnumerator() => _data.GetEnumerator();

    /// <summary>
    /// Adds a query parameter key-value pair
    /// </summary>
    /// <param name="item">The key-value pair to add</param>
    public void Add(KeyValuePair<string, StringValues> item) => _data.Add(item);

    /// <summary>
    /// Removes all query parameters
    /// </summary>
    public void Clear() => _data.Clear();

    /// <summary>
    /// Determines whether the query contains a specific key-value pair
    /// </summary>
    /// <param name="item">The key-value pair to locate</param>
    /// <returns>True if the item is found; otherwise, false</returns>
    public bool Contains(KeyValuePair<string, StringValues> item) => _data.Contains(item);

    /// <summary>
    /// Copies the query parameters to an array
    /// </summary>
    /// <param name="array">The array to copy to</param>
    /// <param name="arrayIndex">The starting index in the array</param>
    public void CopyTo(KeyValuePair<string, StringValues>[] array, int arrayIndex) => _data.Clear();

    /// <summary>
    /// Removes a specific key-value pair from the query
    /// </summary>
    /// <param name="item">The key-value pair to remove</param>
    /// <returns>True if the item was removed; otherwise, false</returns>
    public bool Remove(KeyValuePair<string, StringValues> item) => _data.Remove(item);

    /// <summary>
    /// Gets the number of query parameters
    /// </summary>
    int ICollection<KeyValuePair<string, StringValues>>.Count => _data.Count;

    /// <summary>
    /// Gets a value indicating whether the query is read-only
    /// </summary>
    public bool IsReadOnly => false;

    /// <summary>
    /// Adds a query parameter with the specified key and values
    /// </summary>
    /// <param name="key">The parameter name</param>
    /// <param name="value">The parameter values</param>
    public void Add(string key, StringValues value) => _data.Add(key, value);

    /// <summary>
    /// Determines whether the query contains the specified key
    /// </summary>
    /// <param name="key">The key to locate</param>
    /// <returns>True if the key is found; otherwise, false</returns>
    bool IDictionary<string, StringValues>.ContainsKey(string key) => _data.ContainsKey(key);

    /// <summary>
    /// Attempts to get the values associated with the specified key
    /// </summary>
    /// <param name="key">The key to locate</param>
    /// <param name="value">When this method returns, contains the values associated with the key, if found</param>
    /// <returns>True if the key was found; otherwise, false</returns>
    bool IReadOnlyDictionary<string, StringValues>.TryGetValue(string key, out StringValues value) =>
        _data.TryGetValue(key, out value);

    /// <summary>
    /// Removes a query parameter with the specified key
    /// </summary>
    /// <param name="key">The parameter name to remove</param>
    /// <returns>True if the parameter was removed; otherwise, false</returns>
    public bool Remove(string key) => _data.Remove(key);

    /// <summary>
    /// Determines whether the query contains the specified key
    /// </summary>
    /// <param name="key">The key to locate</param>
    /// <returns>True if the key is found; otherwise, false</returns>
    bool IReadOnlyDictionary<string, StringValues>.ContainsKey(string key) => _data.ContainsKey(key);

    /// <summary>
    /// Attempts to get the values associated with the specified key
    /// </summary>
    /// <param name="key">The key to locate</param>
    /// <param name="value">When this method returns, contains the values associated with the key, if found</param>
    /// <returns>True if the key was found; otherwise, false</returns>
    bool IDictionary<string, StringValues>.TryGetValue(string key, out StringValues value) =>
        _data.TryGetValue(key, out value);

    /// <summary>
    /// Gets the keys of the query parameters
    /// </summary>
    IEnumerable<string> IReadOnlyDictionary<string, StringValues>.Keys => _data.Keys;

    /// <summary>
    /// Gets the values of the query parameters
    /// </summary>
    IEnumerable<StringValues> IReadOnlyDictionary<string, StringValues>.Values => _data.Values;

    /// <summary>
    /// Gets the keys of the query parameters
    /// </summary>
    ICollection<string> IDictionary<string, StringValues>.Keys => _data.Keys;

    /// <summary>
    /// Gets the values of the query parameters
    /// </summary>
    ICollection<StringValues> IDictionary<string, StringValues>.Values => _data.Values;

    /// <summary>
    /// Gets the number of query parameters
    /// </summary>
    int IReadOnlyCollection<KeyValuePair<string, StringValues>>.Count => _data.Count;

    /// <summary>
    /// Gets or sets the values for the specified parameter key
    /// </summary>
    /// <param name="key">The parameter name</param>
    /// <returns>The parameter values</returns>
    public StringValues this[string key]
    {
        get => _data[key];
        set => _data[key] = value;
    }

    /// <summary>
    /// Determines whether this UriQuery is equal to another UriQuery
    /// </summary>
    /// <param name="other">The UriQuery to compare with</param>
    /// <returns>True if the queries are equal; otherwise, false</returns>
    public bool Equals(UriQuery? other) => other is not null && GetHashCode() == other.GetHashCode();

    /// <summary>
    /// Creates a copy of this UriQuery instance
    /// </summary>
    /// <returns>A new UriQuery instance with the same parameters</returns>
    public UriQuery Copy() => new(_data.ToDictionary(x => x.Key, x => x.Value));

    /// <summary>
    /// Gets the hash code for this UriQuery instance
    /// </summary>
    /// <returns>The hash code</returns>
    public override int GetHashCode()
    {
        var code = 0;

        foreach (var (key, values) in _data.OrderBy(x => x.Key))
        foreach (var value in values.OrderBy(x => x))
        {
            code = HashCode.Combine(code, key, value);
        }

        return code;
    }

    /// <summary>
    /// Converts the query parameters to a query string format
    /// </summary>
    /// <returns>The formatted query string</returns>
    public override string ToString()
    {
        var builder = new StringBuilder();

        var joiner = '?';
        foreach (var (key, values) in _data)
        {
            if (values.Count == 0)
            {
                Append(builder, joiner, key, string.Empty);
                joiner = '&';
                continue;
            }

            foreach (var value in values)
            {
                Append(builder, joiner, key, value);
                joiner = '&';
            }
        }

        return builder.ToString();

        static void Append(StringBuilder b, char joiner, string key, string? value)
        {
            b.Append(joiner);
            b.Append(Uri.EscapeDataString(key));
            b.Append('=');
            b.Append(Uri.EscapeDataString(value ?? string.Empty));
        }
    }
}
