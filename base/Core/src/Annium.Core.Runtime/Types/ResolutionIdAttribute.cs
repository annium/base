using System;

namespace Annium.Core.Runtime.Types;

/// <summary>
/// Marks a property as the resolution identifier for a type.
/// </summary>
/// <remarks>
/// This attribute is used to mark properties that contain the type's unique identifier string,
/// which is obtained using Type.GetIdString(). The marked property is used by the type manager
/// for type resolution operations.
///
/// Example usage:
/// <code>
/// public class MyHandler
/// {
///     [ResolutionId]
///     public string Id { get; } = typeof(MyHandler).GetIdString();
/// }
/// </code>
/// </remarks>
[AttributeUsage(AttributeTargets.Property)]
public class ResolutionIdAttribute : Attribute;
