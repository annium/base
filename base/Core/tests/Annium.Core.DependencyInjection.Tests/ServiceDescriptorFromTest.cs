using System;
using System.Reflection;
using Annium.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using MicrosoftServiceDescriptor = Microsoft.Extensions.DependencyInjection.ServiceDescriptor;
using MicrosoftServiceLifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime;

namespace Annium.Core.DependencyInjection.Tests;

/// <summary>
/// Tests for the ServiceDescriptor.From(MicrosoftServiceDescriptor) bypass paths.
/// The bypass paths exist because the public <c>Instance</c> / <c>KeyedInstance</c> factories
/// enforce Singleton via <c>EnsureSingleton</c>, but descriptors that arrive via
/// <c>From(MicrosoftServiceDescriptor)</c> may carry a non-Singleton lifetime set internally
/// by MS DI.  We force that state via reflection so the bypass code path is reachable in tests.
/// </summary>
public class ServiceDescriptorFromTest
{
    /// <summary>
    /// Verifies that From(MicrosoftServiceDescriptor) returns an IInstanceServiceDescriptor even
    /// when the underlying descriptor carries a non-Singleton lifetime — bypassing EnsureSingleton.
    /// </summary>
    [Fact]
    public void From_NonKeyedInstance_BypassesEnsureSingleton()
    {
        // arrange — create a normally-Singleton instance descriptor, then mutate its lifetime field
        // via reflection to simulate a non-Singleton descriptor that could arrive from an MS DI
        // pipeline that does not run the EnsureSingleton guard itself.
        var instance = new SampleService();
        var msDescriptor = new MicrosoftServiceDescriptor(typeof(SampleService), instance);
        ForceLifetime(msDescriptor, MicrosoftServiceLifetime.Scoped);

        // act
        var descriptor = ServiceDescriptor.From(msDescriptor);

        // assert — returned as an instance descriptor (not a type or factory descriptor)
        ((object)descriptor).IsNotDefault();
        var instanceDescriptor = descriptor.As<IInstanceServiceDescriptor>();
        instanceDescriptor.ServiceType.Is(typeof(SampleService));
        instanceDescriptor.ImplementationInstance.Is(instance);
        instanceDescriptor.Lifetime.Is(ServiceLifetime.Scoped);
    }

    /// <summary>
    /// Verifies that From(MicrosoftServiceDescriptor) returns an IKeyedInstanceServiceDescriptor
    /// for a keyed instance descriptor with a non-Singleton lifetime — bypassing EnsureSingleton.
    /// </summary>
    [Fact]
    public void From_KeyedInstance_BypassesEnsureSingleton()
    {
        // arrange — create a keyed instance descriptor and override its lifetime via reflection
        var instance = new SampleService();
        const string key = "k";
        var msDescriptor = new MicrosoftServiceDescriptor(typeof(SampleService), key, instance);
        ForceLifetime(msDescriptor, MicrosoftServiceLifetime.Transient);

        // act
        var descriptor = ServiceDescriptor.From(msDescriptor);

        // assert
        ((object)descriptor).IsNotDefault();
        var keyedDescriptor = descriptor.As<IKeyedInstanceServiceDescriptor>();
        keyedDescriptor.ServiceType.Is(typeof(SampleService));
        keyedDescriptor.Key.Is(key);
        keyedDescriptor.ImplementationInstance.Is(instance);
        keyedDescriptor.Lifetime.Is(ServiceLifetime.Transient);
    }

    /// <summary>
    /// Forces the <c>Lifetime</c> backing field of a <see cref="MicrosoftServiceDescriptor"/>
    /// to <paramref name="lifetime"/> using reflection.  This is the only way to create an
    /// instance-bearing descriptor with a non-Singleton lifetime because every public MS DI
    /// constructor that accepts an instance object hard-codes Singleton.
    /// </summary>
    /// <param name="descriptor">The descriptor whose lifetime to override.</param>
    /// <param name="lifetime">The desired (non-Singleton) lifetime to inject.</param>
    private static void ForceLifetime(MicrosoftServiceDescriptor descriptor, MicrosoftServiceLifetime lifetime)
    {
        // Try well-known field names in order of likelihood across MS DI versions.
        // .NET 9/10: auto-property backing field is "<Lifetime>k__BackingField"
        // Older versions: "_lifetime"
        string[] candidates = ["<Lifetime>k__BackingField", "_lifetime"];
        FieldInfo? field = null;
        foreach (var name in candidates)
        {
            field = typeof(MicrosoftServiceDescriptor).GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
            if (field is not null)
                break;
        }

        if (field is not null)
        {
            field.SetValue(descriptor, lifetime);
            return;
        }

        // Last resort: private setter on the Lifetime property
        var setter = typeof(MicrosoftServiceDescriptor)
            .GetProperty("Lifetime", BindingFlags.Public | BindingFlags.Instance)
            ?.GetSetMethod(nonPublic: true);
        if (setter is not null)
        {
            setter.Invoke(descriptor, [(object)lifetime]);
            return;
        }

        throw new InvalidOperationException(
            "Could not locate Lifetime backing field or private setter on MicrosoftServiceDescriptor. "
                + "This test requires reflection access to MS DI internals to simulate a non-Singleton "
                + "instance descriptor, which is not possible with this version of M.E.DI."
        );
    }

    /// <summary>
    /// Minimal service type used only to populate test descriptors.
    /// </summary>
    private sealed class SampleService;
}
