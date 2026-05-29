using System.Collections.Generic;
using Annium.Core.DependencyInjection.Internal.Builders.Registrations;

namespace Annium.Core.DependencyInjection.Internal.Builders;

/// <summary>
/// Shared scaffolding for the five fluent registration builders (Single, Bulk, Factory, KeyedFactory, Instance).
/// Owns the container reference, registrar, accumulated registrations and the
/// <see cref="RegistrationsInitiated"/> guard, so subclasses keep only the kind-specific state
/// (e.g. type, instance, factory, key) and their <c>AsX</c> + lifetime methods.
/// </summary>
internal abstract class RegistrationBuilderBase
{
    /// <summary>
    /// Message thrown when a lifetime terminator (<c>In</c>/<c>Singleton</c>/<c>Scoped</c>/
    /// <c>Transient</c>) is called before any <c>AsX</c> registration target was specified.
    /// </summary>
    protected const string NoRegistrationTargetsMessage = "Specify registration targets";

    /// <summary>The service container the lifetime call ultimately registers against.</summary>
    protected readonly IServiceContainer Container;

    /// <summary>The registrar responsible for translating registrations + lifetime into descriptors.</summary>
    protected readonly Registrar Registrar;

    /// <summary>The collection of registrations accumulated during the AsX phase.</summary>
    protected readonly List<IRegistration> Registrations = new();

    /// <summary>
    /// Whether at least one <c>AsX</c> call has populated <see cref="Registrations"/>. Lifetime
    /// methods use this to fail fast when the caller forgot to specify registration targets.
    /// </summary>
    protected bool RegistrationsInitiated;

    /// <summary>
    /// Initializes the shared state.
    /// </summary>
    /// <param name="container">The service container the registration is ultimately applied to.</param>
    /// <param name="registrar">The registrar that emits descriptors for the chosen lifetime.</param>
    protected RegistrationBuilderBase(IServiceContainer container, Registrar registrar)
    {
        Container = container;
        Registrar = registrar;
    }

    /// <summary>
    /// Records a registration and flips <see cref="RegistrationsInitiated"/>.
    /// </summary>
    /// <param name="registration">The registration to record.</param>
    protected void Track(IRegistration registration)
    {
        RegistrationsInitiated = true;
        Registrations.Add(registration);
    }

    /// <summary>
    /// Records a batch of registrations and flips <see cref="RegistrationsInitiated"/>.
    /// </summary>
    /// <param name="registrations">The registrations to record.</param>
    protected void Track(IEnumerable<IRegistration> registrations)
    {
        RegistrationsInitiated = true;
        Registrations.AddRange(registrations);
    }
}
