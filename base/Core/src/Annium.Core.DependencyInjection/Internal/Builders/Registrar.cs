using System;
using System.Linq;

namespace Annium.Core.DependencyInjection.Internal.Builders;

internal class Registrar
{
    private readonly Action<IServiceDescriptor> _register;
    private bool _hasRegistered;

    public Registrar(Action<IServiceDescriptor> register)
    {
        _register = register;
    }

    public void Register(
        RegistrationsCollection registrations,
        ServiceLifetime lifetime
    )
    {
        if (_hasRegistered)
            throw new InvalidOperationException("Registration already done");
        _hasRegistered = true;

        if (!registrations.IsInitiated)
            throw new InvalidOperationException("Specify registration targets");

        var descriptors = registrations
            .SelectMany(x => x.ResolveServiceDescriptors(lifetime))
            .ToArray();
        foreach (var descriptor in descriptors)
            _register(descriptor);
    }
}