using System;
using Annium.Core.DependencyInjection;

namespace Annium.Core.Mapper.Attributes;

/// <summary>
/// Is used in combination with startup Assembly scanning by <see cref="Annium.Core.Runtime.Internal.Types.AssembliesCollector"/>
/// inside <see cref="ServiceContainerExtensions.ResolveProfiles"/> method to resolve only really needed generic profile types and register them
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface | AttributeTargets.Enum)]
public class AutoMappedAttribute : Attribute
{
}