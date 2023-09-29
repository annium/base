using System;
using System.Collections.Concurrent;

namespace Annium.Core.Runtime.Types;

public static class TypeExtensions
{
    private static readonly ConcurrentDictionary<Type, TypeId> TypeIds = new();
    public static TypeId GetTypeId(this Type type) => TypeIds.GetOrAdd(type, TypeId.Create);
    public static string GetIdString(this Type type) => TypeId.Create(type).Id;
}