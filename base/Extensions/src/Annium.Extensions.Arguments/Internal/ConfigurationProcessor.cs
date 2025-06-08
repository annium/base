using System;
using System.Linq;
using System.Reflection;

namespace Annium.Extensions.Arguments.Internal;

/// <summary>
/// Processes configuration types to extract property metadata and associated attributes
/// for command line argument parsing and mapping.
/// </summary>
internal class ConfigurationProcessor : IConfigurationProcessor
{
    /// <summary>
    /// Retrieves all writable properties from a type that are decorated with a specific attribute type.
    /// </summary>
    /// <typeparam name="TAttribute">The attribute type to search for, must inherit from BaseAttribute</typeparam>
    /// <param name="type">The configuration type to examine</param>
    /// <returns>Array of tuples containing the property info and associated attribute instance</returns>
    public (PropertyInfo property, TAttribute attribute)[] GetPropertiesWithAttribute<TAttribute>(Type type)
        where TAttribute : BaseAttribute =>
        type.GetProperties()
            .Select(e => (property: e, attribute: e.GetCustomAttribute<TAttribute>()!))
            .Where(e => e.property.CanWrite && e.attribute != null!)
            .ToArray();
}
