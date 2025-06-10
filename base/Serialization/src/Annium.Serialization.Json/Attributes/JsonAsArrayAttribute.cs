using System;

namespace Annium.Serialization.Json.Attributes;

/// <summary>
/// Indicates that a class or struct should be serialized as a JSON array instead of an object.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class JsonAsArrayAttribute : Attribute;
