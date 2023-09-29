using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Annium;

public static class IdExtensions
{
    private static readonly ConcurrentDictionary<Type, IdTable> IdTables = new();

    public static string GetFullId<T>(this T obj) =>
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        obj is null ? "null" : $"{obj.GetType().FriendlyName()}#{obj.GetId()}";

    public static string GetId<T>(this T obj) =>
        obj is null ? "null" : IdTables.GetOrAdd(obj.GetType(), _ => new IdTable()).GetId(obj);

    private class IdTable
    {
        private long _id;
        private readonly ConditionalWeakTable<object, string> _ids = new();

        public string GetId(object obj) =>
            _ids.GetValue(obj, _ => Interlocked.Increment(ref _id).ToString());
    }
}