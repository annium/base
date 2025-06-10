using System;

namespace Annium.Serialization.Json.Attributes;

/// <summary>
/// Indicates that a class or struct should be serialized as compact JSON without indentation.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class JsonNotIndentedAttribute : Attribute;
