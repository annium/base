using System;
using System.Linq;

namespace Annium.Core.DependencyInjection.Internal.Builders;

/// <summary>
/// Handles the registration of service descriptors with the dependency injection container
/// </summary>
internal class Registrar
{
    /// <summary>
    /// The action to register service descriptors
    /// </summary>
    private readonly Action<IServiceDescriptor> _register;

    /// <summary>
    /// Flag indicating whether registration has already been performed
    /// </summary>
    private bool _hasRegistered;

    /// <summary>
    /// Initializes a new instance of the Registrar class
    /// </summary>
    /// <param name="register">The action to register service descriptors</param>
    public Registrar(Action<IServiceDescriptor> register)
    {
        _register = register;
    }

    /// <summary>
    /// Registers all service descriptors from the registrations collection with the specified lifetime
    /// </summary>
    /// <param name="registrations">The collection of registrations to process</param>
    /// <param name="lifetime">The service lifetime to apply</param>
    public void Register(RegistrationsCollection registrations, ServiceLifetime lifetime)
    {
        if (_hasRegistered)
            throw new InvalidOperationException("Registration already done");
        _hasRegistered = true;

        if (!registrations.IsInitiated)
            throw new InvalidOperationException("Specify registration targets");

        var descriptors = registrations.SelectMany(x => x.ResolveServiceDescriptors(lifetime)).ToArray();
        foreach (var descriptor in descriptors)
            _register(descriptor);
    }
}
