using System.Collections.Generic;

namespace Annium.Core.DependencyInjection.Internal.Builders.Registrations;

internal interface IRegistration
{
    IEnumerable<IServiceDescriptor> ResolveServiceDescriptors(ServiceLifetime lifetime);
}
