using System;
using System.Reflection;
using Annium.Extensions.Arguments.Attributes;

namespace Annium.Extensions.Arguments.Internal;

/// <summary>
/// Defines the contract for processing configuration types to extract property metadata and attributes.
/// </summary>
internal interface IConfigurationProcessor
{
    /// <summary>
    /// Retrieves all writable properties from a type that are decorated with a specific attribute type.
    /// </summary>
    /// <typeparam name="TAttribute">The attribute type to search for, must inherit from BaseAttribute</typeparam>
    /// <param name="type">The configuration type to examine</param>
    /// <returns>Array of tuples containing the property info and associated attribute instance</returns>
    (PropertyInfo property, TAttribute attribute)[] GetPropertiesWithAttribute<TAttribute>(Type type)
        where TAttribute : BaseAttribute;
}
