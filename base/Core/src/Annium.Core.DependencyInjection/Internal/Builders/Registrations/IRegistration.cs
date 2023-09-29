using System;
using System.Collections.Generic;

namespace Annium.Core.DependencyInjection.Internal.Builders.Registrations;

internal interface IRegistration
{
    Type ServiceType { get; }
    IEnumerable<IServiceDescriptor> ResolveServiceDescriptors(ServiceLifetime lifetime);
}