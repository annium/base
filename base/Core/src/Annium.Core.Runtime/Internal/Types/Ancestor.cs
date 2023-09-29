using System;
using System.Linq;
using System.Reflection;
using Annium.Core.Runtime.Types;
using Annium.Reflection;

namespace Annium.Core.Runtime.Internal.Types;

internal sealed class Ancestor
{
    public Type Type { get; }
    public PropertyInfo? IdProperty { get; }
    public bool HasIdProperty { get; }
    public PropertyInfo? KeyProperty { get; }
    public bool HasKeyProperty { get; }

    public Ancestor(
        Type type
    )
    {
        var properties = type.GetAllProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
        var idProperties = properties.Where(p => p.GetCustomAttribute<ResolutionIdAttribute>() != null).ToArray();
        var keyProperties = properties.Where(p => p.GetCustomAttribute<ResolutionKeyAttribute>() != null).ToArray();

        if (idProperties.Length > 1)
            throw new ArgumentException($"Type '{type}' has multiple resolution ids defined: {string.Join(", ", idProperties.Select(f => f.Name))}.");

        if (keyProperties.Length > 1)
            throw new ArgumentException($"Type '{type}' has multiple resolution keys defined: {string.Join(", ", keyProperties.Select(f => f.Name))}.");

        Type = type;
        IdProperty = idProperties.FirstOrDefault();
        HasIdProperty = IdProperty != null;
        KeyProperty = keyProperties.FirstOrDefault();
        HasKeyProperty = KeyProperty != null;
    }

    public override string ToString() => Type.Name;
}