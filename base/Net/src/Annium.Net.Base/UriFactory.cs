using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Primitives;

namespace Annium.Net.Base;

/// <summary>
/// Factory for building URIs with fluent configuration
/// </summary>
public class UriFactory
{
    /// <summary>
    /// Creates a new UriFactory with the specified base URI
    /// </summary>
    /// <param name="baseUri">The base URI</param>
    /// <returns>A new UriFactory instance</returns>
    public static UriFactory Base(Uri baseUri) => new(baseUri);

    /// <summary>
    /// Creates a new UriFactory with the specified base URI string
    /// </summary>
    /// <param name="baseUri">The base URI string</param>
    /// <returns>A new UriFactory instance</returns>
    public static UriFactory Base(string baseUri) => new(new Uri(baseUri));

    /// <summary>
    /// Creates a new UriFactory without a base URI
    /// </summary>
    /// <returns>A new UriFactory instance</returns>
    public static UriFactory Base() => new();

    /// <summary>
    /// The base URI for relative path construction
    /// </summary>
    private readonly Uri? _baseUri;

    /// <summary>
    /// The path component of the URI
    /// </summary>
    private string? _uri;

    /// <summary>
    /// The query parameters collection
    /// </summary>
    private readonly UriQuery _query = UriQuery.New();

    private UriFactory(Uri? baseUri, string? uri, UriQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (baseUri != null)
            EnsureAbsolute(baseUri);

        _baseUri = baseUri;
        _uri = uri;
        _query = query.Copy();
    }

    private UriFactory(Uri baseUri)
    {
        ArgumentNullException.ThrowIfNull(baseUri);

        EnsureAbsolute(baseUri);

        _baseUri = baseUri;
    }

    private UriFactory() { }

    /// <summary>
    /// Sets the path component of the URI
    /// </summary>
    /// <param name="uri">The URI path</param>
    /// <returns>This UriFactory instance for method chaining</returns>
    public UriFactory Path(string uri)
    {
        ArgumentNullException.ThrowIfNull(uri);

        _uri = uri.Trim();

        return this;
    }

    /// <summary>
    /// Adds a query parameter with multiple values from a List
    /// </summary>
    /// <typeparam name="T">The type of the parameter values</typeparam>
    /// <param name="key">The parameter name</param>
    /// <param name="values">The parameter values</param>
    /// <returns>This UriFactory instance for method chaining</returns>
    public UriFactory Param<T>(string key, List<T> values) => Param(key, values.AsEnumerable());

    /// <summary>
    /// Adds a query parameter with multiple values from an IList
    /// </summary>
    /// <typeparam name="T">The type of the parameter values</typeparam>
    /// <param name="key">The parameter name</param>
    /// <param name="values">The parameter values</param>
    /// <returns>This UriFactory instance for method chaining</returns>
    public UriFactory Param<T>(string key, IList<T> values) => Param(key, values.AsEnumerable());

    /// <summary>
    /// Adds a query parameter with multiple values from an IReadOnlyList
    /// </summary>
    /// <typeparam name="T">The type of the parameter values</typeparam>
    /// <param name="key">The parameter name</param>
    /// <param name="values">The parameter values</param>
    /// <returns>This UriFactory instance for method chaining</returns>
    public UriFactory Param<T>(string key, IReadOnlyList<T> values) => Param(key, values.AsEnumerable());

    /// <summary>
    /// Adds a query parameter with multiple values from an IReadOnlyCollection
    /// </summary>
    /// <typeparam name="T">The type of the parameter values</typeparam>
    /// <param name="key">The parameter name</param>
    /// <param name="values">The parameter values</param>
    /// <returns>This UriFactory instance for method chaining</returns>
    public UriFactory Param<T>(string key, IReadOnlyCollection<T> values) => Param(key, values.AsEnumerable());

    /// <summary>
    /// Adds a query parameter with multiple values from an array
    /// </summary>
    /// <typeparam name="T">The type of the parameter values</typeparam>
    /// <param name="key">The parameter name</param>
    /// <param name="values">The parameter values</param>
    /// <returns>This UriFactory instance for method chaining</returns>
    public UriFactory Param<T>(string key, T[] values) => Param(key, values.AsEnumerable());

    /// <summary>
    /// Adds a query parameter with multiple values from an IEnumerable
    /// </summary>
    /// <typeparam name="T">The type of the parameter values</typeparam>
    /// <param name="key">The parameter name</param>
    /// <param name="values">The parameter values</param>
    /// <returns>This UriFactory instance for method chaining</returns>
    public UriFactory Param<T>(string key, IEnumerable<T> values)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(values);

        _query[key] = new StringValues(values.Where(x => x is not null).Select(x => x!.ToString()).ToArray());

        return this;
    }

    /// <summary>
    /// Adds a query parameter with a single value
    /// </summary>
    /// <typeparam name="T">The type of the parameter value</typeparam>
    /// <param name="key">The parameter name</param>
    /// <param name="value">The parameter value</param>
    /// <returns>This UriFactory instance for method chaining</returns>
    public UriFactory Param<T>(string key, T value)
    {
        ArgumentNullException.ThrowIfNull(key);

        _query[key] = value?.ToString() ?? string.Empty;

        return this;
    }

    // public UriFactory Param(string key, object? value)
    // {
    //     if (value is System.Collections.IEnumerable enumerable)
    //         _query[key] = new StringValues((from object? item in enumerable select item?.ToString() ?? string.Empty).ToArray());
    //     else
    //         _query[key] = value?.ToString() ?? string.Empty;
    //
    //     return this;
    // }

    /// <summary>
    /// Adds a query parameter with StringValues
    /// </summary>
    /// <param name="key">The parameter name</param>
    /// <param name="value">The parameter values</param>
    /// <returns>This UriFactory instance for method chaining</returns>
    public UriFactory Param(string key, StringValues value)
    {
        ArgumentNullException.ThrowIfNull(key);

        _query[key] = value;

        return this;
    }

    /// <summary>
    /// Creates a copy of this UriFactory instance
    /// </summary>
    /// <returns>A new UriFactory instance with the same configuration</returns>
    public UriFactory Clone() => new(_baseUri, _uri, _query);

    /// <summary>
    /// Builds the final URI from the configured components
    /// </summary>
    /// <returns>The constructed URI</returns>
    public Uri Build()
    {
        var uri = BuildUriBase();
        var query = UriQuery.Parse(uri.Query);

        // add manually defined params to queryBuilder
        foreach (var (name, value) in _query)
            if (!query.TryAdd(name, value))
                query[name] = StringValues.Concat(query[name], value);

        return new UriBuilder(uri) { Query = query.ToString() }.Uri;
    }

    /// <summary>
    /// Builds the base URI from the configured base URI and path
    /// </summary>
    /// <returns>The constructed base URI</returns>
    private Uri BuildUriBase()
    {
        if (_baseUri is null)
        {
            if (string.IsNullOrWhiteSpace(_uri))
                throw new UriFormatException("Request URI is empty");

            var uri = new Uri(_uri);
            EnsureAbsolute(uri);

            return uri;
        }

        if (string.IsNullOrWhiteSpace(_uri) || _uri == "/")
            return _baseUri;

        if (!_uri.StartsWith("/"))
        {
            if (Uri.TryCreate(_uri, UriKind.Absolute, out _))
                throw new UriFormatException("Both base and path are absolute URI");

            return new Uri($"{_baseUri.ToString().TrimEnd('/')}/{_uri.TrimStart('/')}");
        }

        var sb = new StringBuilder();
        sb.Append($"{_baseUri.Scheme}://");
        sb.Append(_baseUri.Host);
        if (!_baseUri.IsDefaultPort)
            sb.Append($":{_baseUri.Port}");

        return new Uri($"{sb}/{_uri.TrimStart('/')}");
    }

    /// <summary>
    /// Ensures that the specified URI is absolute
    /// </summary>
    /// <param name="uri">The URI to validate</param>
    private void EnsureAbsolute(Uri uri)
    {
        if (!uri.IsAbsoluteUri)
            throw new UriFormatException($"URI {uri} is not absolute");
    }
}
