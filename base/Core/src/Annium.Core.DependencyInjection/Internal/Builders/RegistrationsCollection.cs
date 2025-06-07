using System.Collections;
using System.Collections.Generic;
using Annium.Core.DependencyInjection.Internal.Builders.Registrations;

namespace Annium.Core.DependencyInjection.Internal.Builders;

/// <summary>
/// Collection that holds service registrations and tracks initialization state
/// </summary>
internal class RegistrationsCollection : IReadOnlyCollection<IRegistration>
{
    /// <summary>
    /// Gets a value indicating whether the collection has been initialized
    /// </summary>
    public bool IsInitiated { get; private set; }

    /// <summary>
    /// Gets the number of registrations in the collection
    /// </summary>
    public int Count => _registrations.Count;

    /// <summary>
    /// The internal list of registrations
    /// </summary>
    private readonly List<IRegistration> _registrations = new();

    /// <summary>
    /// Initializes the collection and marks it as ready for registration processing
    /// </summary>
    public void Init() => IsInitiated = true;

    /// <summary>
    /// Adds a registration to the collection
    /// </summary>
    /// <param name="registration">The registration to add</param>
    public void Add(IRegistration registration) => _registrations.Add(registration);

    /// <summary>
    /// Adds multiple registrations to the collection
    /// </summary>
    /// <param name="registrations">The registrations to add</param>
    public void AddRange(IEnumerable<IRegistration> registrations) => _registrations.AddRange(registrations);

    /// <summary>
    /// Returns an enumerator that iterates through the registrations
    /// </summary>
    /// <returns>An enumerator for the registrations</returns>
    public IEnumerator<IRegistration> GetEnumerator() => _registrations.GetEnumerator();

    /// <summary>
    /// Returns an enumerator that iterates through the registrations
    /// </summary>
    /// <returns>An enumerator for the registrations</returns>
    IEnumerator IEnumerable.GetEnumerator() => _registrations.GetEnumerator();
}
