using System.Collections;
using System.Collections.Generic;
using Annium.Core.DependencyInjection.Internal.Builders.Registrations;

namespace Annium.Core.DependencyInjection.Internal.Builders;

internal class RegistrationsCollection : IReadOnlyCollection<IRegistration>
{
    public bool IsInitiated => _isInitiated;
    public int Count => _registrations.Count;
    private readonly List<IRegistration> _registrations = new();
    private bool _isInitiated;

    public void Init() => _isInitiated = true;

    public void Add(IRegistration registration) => _registrations.Add(registration);
    public void AddRange(IEnumerable<IRegistration> registrations) => _registrations.AddRange(registrations);

    public IEnumerator<IRegistration> GetEnumerator() => _registrations.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _registrations.GetEnumerator();
}