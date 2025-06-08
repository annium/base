using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Annium;

/// <summary>
/// Provides extension methods for generating unique identifiers for objects.
/// </summary>
public static class IdExtensions
{
    /// <summary>
    /// A thread-safe dictionary that maps types to their ID tables.
    /// </summary>
    private static readonly ConcurrentDictionary<Type, IdTable> _idTables = new();

    /// <summary>
    /// Gets a full identifier for an object, including its type name and instance ID.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <param name="obj">The object to get the identifier for.</param>
    /// <returns>A string in the format "TypeName#InstanceId". Returns "null" if the object is null.</returns>
    public static string GetFullId<T>(this T obj) =>
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        obj is null
            ? "null"
            : $"{obj.GetType().FriendlyName()}#{obj.GetId()}";

    /// <summary>
    /// Gets a unique identifier for an object instance.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <param name="obj">The object to get the identifier for.</param>
    /// <returns>A unique string identifier for the object. Returns "null" if the object is null.</returns>
    public static string GetId<T>(this T obj) =>
        obj is null ? "null" : _idTables.GetOrAdd(obj.GetType(), _ => new IdTable()).GetId(obj);

    /// <summary>
    /// A thread-safe table that maintains unique identifiers for objects of a specific type.
    /// </summary>
    private class IdTable
    {
        /// <summary>
        /// The next available ID value.
        /// </summary>
        private long _id;

        /// <summary>
        /// A weak reference table that maps objects to their IDs.
        /// </summary>
        private readonly ConditionalWeakTable<object, string> _ids = new();

        /// <summary>
        /// Gets or creates a unique identifier for an object.
        /// </summary>
        /// <param name="obj">The object to get the identifier for.</param>
        /// <returns>A unique string identifier for the object.</returns>
        public string GetId(object obj) => _ids.GetValue(obj, _ => Interlocked.Increment(ref _id).ToString());
    }
}
