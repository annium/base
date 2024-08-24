using System.Collections;
using System.Collections.Generic;
using Annium.Core.DependencyInjection.Internal.Builders.Registrations;

namespace Annium.Core.DependencyInjection.Internal.Builders;

internal class RegistrationsCollection : IReadOnlyCollection<IRegistration>
{
    public bool IsInitiated { get; private set; }
    public int Count => _registrations.Count;
    private readonly List<IRegistration> _registrations = new();

    public void Init() => IsInitiated = true;

    public void Add(IRegistration registration) => _registrations.Add(registration);

    public void AddRange(IEnumerable<IRegistration> registrations) => _registrations.AddRange(registrations);

    public IEnumerator<IRegistration> GetEnumerator() => _registrations.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _registrations.GetEnumerator();
}
