using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Primitives;

namespace Annium.Net.Base;

public class UriFactory
{
    public static UriFactory Base(Uri baseUri) => new(baseUri);

    public static UriFactory Base(string baseUri) => new(new Uri(baseUri));

    public static UriFactory Base() => new();

    private readonly Uri? _baseUri;
    private string? _uri;
    private readonly UriQuery _query = UriQuery.New();

    private UriFactory(
        Uri? baseUri,
        string? uri,
        UriQuery query
    )
    {
        if (baseUri != null)
            EnsureAbsolute(baseUri);

        _baseUri = baseUri;
        _uri = uri;
        _query = query.Copy();
    }

    private UriFactory(
        Uri baseUri
    )
    {
        EnsureAbsolute(baseUri);

        _baseUri = baseUri;
    }

    private UriFactory()
    {
    }

    public UriFactory Path(string uri)
    {
        _uri = uri.Trim();

        return this;
    }

    public UriFactory Param<T>(string key, List<T> values) => Param(key, values.AsEnumerable());
    public UriFactory Param<T>(string key, IList<T> values) => Param(key, values.AsEnumerable());
    public UriFactory Param<T>(string key, IReadOnlyList<T> values) => Param(key, values.AsEnumerable());
    public UriFactory Param<T>(string key, IReadOnlyCollection<T> values) => Param(key, values.AsEnumerable());
    public UriFactory Param<T>(string key, T[] values) => Param(key, values.AsEnumerable());

    public UriFactory Param<T>(string key, IEnumerable<T> values)
    {
        _query[key] = new StringValues(values.Where(x => x is not null).Select(x => x!.ToString()).ToArray());

        return this;
    }

    public UriFactory Param<T>(string key, T value)
    {
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

    public UriFactory Param(string key, StringValues value)
    {
        _query[key] = value;

        return this;
    }

    public UriFactory Clone() => new(_baseUri, _uri, _query);

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

    private void EnsureAbsolute(Uri uri)
    {
        if (!uri.IsAbsoluteUri)
            throw new UriFormatException($"URI {uri} is not absolute");
    }
}