using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using Annium.Core.Runtime.Internal.Types;
using Annium.Logging;

namespace Annium.Core.Runtime.Types;

/// <summary>
/// Provides static methods for managing type instances across the application.
/// </summary>
/// <remarks>
/// This class implements a singleton pattern for type managers, ensuring that each assembly
/// has only one type manager instance. It's used for type resolution and management throughout
/// the application.
/// </remarks>
public static class TypeManager
{
    /// <summary>
    /// Cache of type manager instances indexed by assembly cache key
    /// </summary>
    private static readonly ConcurrentDictionary<CacheKey, ITypeManager> _instances = new();

    /// <summary>
    /// Gets or creates a type manager instance for the specified assembly.
    /// </summary>
    /// <param name="assembly">The assembly to create a type manager for.</param>
    /// <param name="logger">The logger instance to use for type manager operations.</param>
    /// <returns>A type manager instance for the specified assembly.</returns>
    /// <remarks>
    /// This method ensures that only one type manager instance exists per assembly.
    /// Example usage:
    /// <code>
    /// var typeManager = TypeManager.GetInstance(typeof(Program).Assembly, logger);
    /// </code>
    /// </remarks>
    public static ITypeManager GetInstance(Assembly assembly, ILogger logger)
    {
        var key = new CacheKey(assembly);
        return _instances.GetOrAdd(key, k => new TypeManagerInstance(k.Assembly, logger));
    }

    /// <summary>
    /// Releases all type manager instances associated with the specified assembly.
    /// </summary>
    /// <param name="assembly">The assembly whose type manager instances should be released.</param>
    /// <remarks>
    /// This method is typically called during application shutdown or when an assembly is unloaded.
    /// Example usage:
    /// <code>
    /// TypeManager.Release(typeof(Program).Assembly);
    /// </code>
    /// </remarks>
    public static void Release(Assembly assembly)
    {
        foreach (var key in _instances.Keys.Where(x => x.Assembly == assembly).ToArray())
            _instances.TryRemove(key, out _);
    }

    /// <summary>
    /// Represents a key for caching type manager instances.
    /// </summary>
    /// <param name="Assembly">The assembly associated with the type manager instance.</param>
    /// <remarks>
    /// This record is used internally to uniquely identify type manager instances in the cache.
    /// </remarks>
    private record CacheKey(Assembly Assembly)
    {
        /// <summary>
        /// Determines whether two cache keys are equal based on their hash codes.
        /// </summary>
        /// <param name="other">The other cache key to compare with.</param>
        /// <returns>True if the hash codes are equal; otherwise, false.</returns>
        public virtual bool Equals(CacheKey? other) => GetHashCode() == other?.GetHashCode();

        /// <summary>
        /// Gets the hash code for this cache key.
        /// </summary>
        /// <returns>A hash code based on the assembly.</returns>
        public override int GetHashCode() => HashCode.Combine(Assembly);
    }
}
