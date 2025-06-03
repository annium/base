using System;

namespace Annium.Core.Runtime.Types;

/// <summary>
/// Marks a property as the resolution key for a type.
/// </summary>
/// <remarks>
/// This attribute is used to mark properties that contain a key used to identify and resolve types.
/// The marked property is used by the type manager for type resolution operations using keys.
///
/// Example usage:
/// <code>
/// public class MyHandler
/// {
///     [ResolutionKey]
///     public string Key { get; } = "MyHandlerKey";
/// }
/// </code>
/// </remarks>
[AttributeUsage(AttributeTargets.Property)]
public class ResolutionKeyAttribute : Attribute;
