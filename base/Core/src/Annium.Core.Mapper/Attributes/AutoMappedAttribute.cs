using System;

namespace Annium.Core.Mapper.Attributes;

/// <summary>
/// Used in combination with startup assembly scanning by <see cref="Runtime.Internal.Types.AssembliesCollector"/>
/// inside <see cref="Internal.MapperRegistration"/> to resolve only the generic profile closures whose
/// type arguments are themselves opted-in via this attribute, and register them.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface | AttributeTargets.Enum)]
public class AutoMappedAttribute : Attribute;
