using System;
using System.Collections.Generic;
using System.Reflection;

namespace Annium.Serialization.Json.Internal.Converters;

/// <summary>
/// Configuration for constructor-based JSON conversion, containing constructor and parameter information.
/// </summary>
internal class ConstructorJsonConverterConfiguration
{
    /// <summary>
    /// Gets the constructor to use for object creation.
    /// </summary>
    public ConstructorInfo Constructor { get; }

    /// <summary>
    /// Gets the list of constructor parameters.
    /// </summary>
    public List<ParameterItem> Parameters { get; }

    /// <summary>
    /// Gets the collection of properties that can be set after construction.
    /// </summary>
    public IReadOnlyCollection<PropertyInfo> Properties { get; }

    /// <summary>
    /// Initializes a new instance of the ConstructorJsonConverterConfiguration class.
    /// </summary>
    /// <param name="constructor">The constructor to use for object creation.</param>
    /// <param name="parameters">The list of constructor parameters.</param>
    /// <param name="properties">The collection of properties that can be set after construction.</param>
    public ConstructorJsonConverterConfiguration(
        ConstructorInfo constructor,
        List<ParameterItem> parameters,
        IReadOnlyCollection<PropertyInfo> properties
    )
    {
        Constructor = constructor;
        Parameters = parameters;
        Properties = properties;
    }

    /// <summary>
    /// Represents a constructor parameter with its type and name information.
    /// </summary>
    internal class ParameterItem
    {
        /// <summary>
        /// Gets the parameter type.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Gets the parameter name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Initializes a new instance of the ParameterItem class.
        /// </summary>
        /// <param name="type">The parameter type.</param>
        /// <param name="name">The parameter name.</param>
        public ParameterItem(Type type, string name)
        {
            Type = type;
            Name = name;
        }

        /// <summary>
        /// Returns a string representation of the parameter.
        /// </summary>
        /// <returns>A string containing the parameter type and name.</returns>
        public override string ToString() => $"{Type.FriendlyName()} {Name}";
    }
}
