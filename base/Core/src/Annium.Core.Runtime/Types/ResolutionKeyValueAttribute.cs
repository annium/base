using System;

namespace Annium.Core.Runtime.Types;

/// <summary>
/// Specifies a key value for type resolution at the class level.
/// </summary>
/// <remarks>
/// This attribute is used to mark classes with a specific key value that can be used
/// for type resolution. It works in conjunction with properties marked with ResolutionKeyAttribute.
///
/// Example usage:
/// <code>
/// [ResolutionKeyValue("MyHandlerKey")]
/// public class MyHandler
/// {
///     [ResolutionKey]
///     public string Key { get; } = "MyHandlerKey";
/// }
/// </code>
/// </remarks>
[AttributeUsage(AttributeTargets.Class)]
public class ResolutionKeyValueAttribute : Attribute
{
    /// <summary>
    /// Gets the key value associated with this attribute.
    /// </summary>
    /// <remarks>
    /// This value is used by the type manager for type resolution operations.
    /// </remarks>
    public object Key { get; }

    /// <summary>
    /// Initializes a new instance of the ResolutionKeyValueAttribute class.
    /// </summary>
    /// <param name="key">The key value to associate with the class.</param>
    /// <exception cref="ArgumentNullException">Thrown when the key parameter is null.</exception>
    /// <remarks>
    /// The key value must not be null and should match the value of the property
    /// marked with ResolutionKeyAttribute in the class.
    /// </remarks>
    public ResolutionKeyValueAttribute(object key)
    {
        Key = key ?? throw new ArgumentNullException(nameof(key));
    }
}
