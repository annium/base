using System;

namespace Annium.Core.Runtime.Types;

/// <summary>
/// Property, marked by this attribute, must contain Type.GetIdString() string value
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class ResolutionIdAttribute : Attribute
{
}