using System;

namespace Annium.Serialization.Json.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class JsonNotIndentedAttribute : Attribute
{
}