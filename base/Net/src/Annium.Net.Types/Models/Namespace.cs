using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Annium.Net.Types.Extensions;

namespace Annium.Net.Types.Models;

public sealed record Namespace : IReadOnlyList<string>
{
    #region static

    public static Namespace New(IEnumerable<string> ns) => new(ns.ToArray().EnsureValidNamespace());
    public static Namespace New(IReadOnlyList<string> ns) => new(ns.EnsureValidNamespace());

    #endregion

    #region instance

    public int Count => _parts.Count;
    public string this[int index] => _parts[index];
    private readonly IReadOnlyList<string> _parts;

    private Namespace(IReadOnlyList<string> parts)
    {
        _parts = parts;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public IEnumerator<string> GetEnumerator() => _parts.GetEnumerator();

    public override string ToString() => _parts.ToNamespaceString();

    public bool Equals(Namespace? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return _parts.SequenceEqual(other._parts);
    }

    public override int GetHashCode() => HashCodeSeq.Combine(_parts);

    #endregion

    #region operators

    public static implicit operator Namespace(string value) => value.ToNamespace();
    public static implicit operator string(Namespace value) => value.ToString();

    #endregion
}