using System;

namespace Annium.Core.Runtime.Types;

/// <summary>
/// Property, marked by this attribute, defines property, containing type identifying key
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class ResolutionKeyAttribute : Attribute
{
}