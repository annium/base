using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using Annium.Core.Runtime.Internal.Types;
using Annium.Logging;

namespace Annium.Core.Runtime.Types;

public static class TypeManager
{
    private static readonly ConcurrentDictionary<CacheKey, ITypeManager> _instances = new();

    public static ITypeManager GetInstance(Assembly assembly, ILogger logger)
    {
        var key = new CacheKey(assembly);
        return _instances.GetOrAdd(key, k => new TypeManagerInstance(k.Assembly, logger));
    }

    public static void Release(Assembly assembly)
    {
        foreach (var key in _instances.Keys.Where(x => x.Assembly == assembly).ToArray())
            _instances.TryRemove(key, out _);
    }

    private record CacheKey(Assembly Assembly)
    {
        public virtual bool Equals(CacheKey? other) => GetHashCode() == other?.GetHashCode();

        public override int GetHashCode() => HashCode.Combine(Assembly);
    }
}
