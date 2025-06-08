using System;
using System.Linq;
using System.Reflection;
using Annium.Core.Runtime.Types;
using Annium.Reflection;

namespace Annium.Core.Runtime.Internal.Types;

/// <summary>
/// Internal representation of an ancestor type in the type hierarchy
/// </summary>
internal sealed class Ancestor
{
    /// <summary>
    /// The ancestor type
    /// </summary>
    public Type Type { get; }

    /// <summary>
    /// The property marked with ResolutionIdAttribute, if any
    /// </summary>
    public PropertyInfo? IdProperty { get; }

    /// <summary>
    /// Whether this ancestor has an ID property for resolution
    /// </summary>
    public bool HasIdProperty { get; }

    /// <summary>
    /// The property marked with ResolutionKeyAttribute, if any
    /// </summary>
    public PropertyInfo? KeyProperty { get; }

    /// <summary>
    /// Whether this ancestor has a key property for resolution
    /// </summary>
    public bool HasKeyProperty { get; }

    /// <summary>
    /// Initializes a new instance of Ancestor for the specified type
    /// </summary>
    /// <param name="type">The type to analyze for resolution properties</param>
    public Ancestor(Type type)
    {
        var properties = type.GetAllProperties(
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy
        );
        var idProperties = properties.Where(p => p.GetCustomAttribute<ResolutionIdAttribute>() != null).ToArray();
        var keyProperties = properties.Where(p => p.GetCustomAttribute<ResolutionKeyAttribute>() != null).ToArray();

        if (idProperties.Length > 1)
            throw new ArgumentException(
                $"Type '{type}' has multiple resolution ids defined: {string.Join(", ", idProperties.Select(f => f.Name))}."
            );

        if (keyProperties.Length > 1)
            throw new ArgumentException(
                $"Type '{type}' has multiple resolution keys defined: {string.Join(", ", keyProperties.Select(f => f.Name))}."
            );

        Type = type;
        IdProperty = idProperties.FirstOrDefault();
        HasIdProperty = IdProperty != null;
        KeyProperty = keyProperties.FirstOrDefault();
        HasKeyProperty = KeyProperty != null;
    }

    /// <summary>
    /// Returns the string representation of this ancestor
    /// </summary>
    /// <returns>The name of the ancestor type</returns>
    public override string ToString() => Type.Name;
}
