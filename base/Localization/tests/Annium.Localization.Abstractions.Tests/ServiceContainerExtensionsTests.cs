using System;
using System.Globalization;
using Annium.Core.DependencyInjection;
using Annium.Localization.Abstractions;
using Annium.Localization.InMemory;
using Annium.Testing;
using Xunit;

namespace Annium.Localization.Abstractions.Tests;

/// <summary>
/// Tests for <see cref="ServiceContainerExtensions.AddLocalization"/> registration behaviour:
/// validation, duplicate-storage guard, and singleton lifetime.
/// </summary>
public class ServiceContainerExtensionsTests : IDisposable
{
    /// <summary>
    /// Captures the ambient culture so each test can restore it, preventing
    /// CultureInfo.CurrentCulture mutations from leaking across tests.
    /// </summary>
    private readonly CultureInfo _savedCulture = CultureInfo.CurrentCulture;

    /// <summary>
    /// Restores the ambient culture mutated during the test.
    /// </summary>
    public void Dispose()
    {
        CultureInfo.CurrentCulture = _savedCulture;
    }

    /// <summary>
    /// Verifies that <see cref="ServiceContainerExtensions.AddLocalization"/> throws
    /// <see cref="InvalidOperationException"/> when the configure delegate does not register
    /// any storage backend.
    /// </summary>
    [Fact]
    public void AddLocalization_NoStorage_Throws()
    {
        Wrap.It(() => new ServiceContainer().AddLocalization(_ => { })).Throws<InvalidOperationException>();
    }

    /// <summary>
    /// Verifies that calling <c>UseInMemoryStorage()</c> twice
    /// in the same <c>AddLocalization</c> configure delegate throws <see cref="InvalidOperationException"/>,
    /// because <see cref="LocalizationOptions.SetLocaleStorage"/> enforces a single-storage invariant.
    /// </summary>
    [Fact]
    public void SetLocaleStorage_CalledTwice_Throws()
    {
        Wrap.It(() =>
                new ServiceContainer().AddLocalization(opts =>
                {
                    opts.UseInMemoryStorage();
                    opts.UseInMemoryStorage();
                })
            )
            .Throws<InvalidOperationException>();
    }

    /// <summary>
    /// Verifies that a configure delegate which throws inside SetLocaleStorage does not poison the
    /// single-storage guard: storage can still be configured afterwards on the same options instance.
    /// </summary>
    [Fact]
    public void SetLocaleStorage_ConfigureThrows_DoesNotBlockLaterConfiguration()
    {
        var container = new ServiceContainer();

        container.AddLocalization(opts =>
        {
            // a failed storage configuration must not mark storage as configured
            try
            {
                opts.SetLocaleStorage(_ => throw new InvalidOperationException("boom"));
            }
            catch (InvalidOperationException)
            {
                // expected — the configure delegate threw
            }

            // a subsequent configuration must succeed (the failed attempt did not poison the guard)
            opts.UseInMemoryStorage();
        });

        var provider = container.BuildServiceProvider();
        var localizer = provider.Resolve<ILocalizer<ServiceContainerExtensionsTests>>();

        // empty in-memory storage → key returned, proving the localizer resolved and works
        localizer["x"].Is("x");

        (provider as IDisposable)?.Dispose();
    }

    /// <summary>
    /// Verifies that <see cref="ILocalizer{T}"/> is registered as a singleton: resolving it twice
    /// from the same provider yields the identical instance.
    /// </summary>
    [Fact]
    public void AddLocalization_LocalizerIsSingleton_SameInstanceReturned()
    {
        // arrange
        var container = new ServiceContainer();
        container.AddLocalization(opts => opts.UseInMemoryStorage());
        var provider = container.BuildServiceProvider();

        // act
        var first = provider.Resolve<ILocalizer<ServiceContainerExtensionsTests>>();
        var second = provider.Resolve<ILocalizer<ServiceContainerExtensionsTests>>();

        // assert
        ReferenceEquals(first, second).IsTrue();

        (provider as IDisposable)?.Dispose();
    }
}
