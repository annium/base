using System;

namespace Annium.Serialization.Abstractions.Attributes;

/// <summary>
/// Marks a constructor to be used for deserialization when multiple constructors are available.
/// </summary>
public class DeserializationConstructorAttribute : Attribute;
