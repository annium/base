using System;
using System.Collections.Generic;
using System.Globalization;
using Annium.Core.DependencyInjection;
using Annium.Localization.Abstractions;
using Annium.Testing;
using Xunit;

namespace Annium.Localization.Abstractions.Tests;

/// <summary>
/// Tests that Localizer caches per culture, consulting the backing storage exactly once per culture.
/// </summary>
public class LocalizerCacheTests : IDisposable
{
    /// <summary>
    /// Captures the ambient culture so each test can restore it, preventing
    /// CultureInfo.CurrentCulture mutations from leaking across tests.
    /// </summary>
    private readonly CultureInfo _savedCulture = CultureInfo.CurrentCulture;

    /// <summary>
    /// The service provider built for the current test, disposed on teardown.
    /// </summary>
    private IServiceProvider? _provider;

    /// <summary>
    /// Restores the ambient culture mutated during the test and disposes the built provider.
    /// </summary>
    public void Dispose()
    {
        CultureInfo.CurrentCulture = _savedCulture;
        (_provider as IDisposable)?.Dispose();
    }

    /// <summary>
    /// Verifies that repeated lookups for the same culture consult the backing storage only once
    /// (the per-culture Lazy-backed cache), not on every indexer access.
    /// </summary>
    [Fact]
    public void Translate_RepeatedLookupSameCulture_LoadsLocaleOnce()
    {
        // arrange
        var storage = new CountingStorage();
        var container = new ServiceContainer();
        container.AddLocalization(opts =>
            opts.SetLocaleStorage(c => c.Add<ILocaleStorage>(storage).AsSelf().Singleton())
        );
        _provider = container.BuildServiceProvider();
        var localizer = _provider.Resolve<ILocalizer<LocalizerCacheTests>>();

        // act — two lookups for the same culture, then one for a different culture
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en");
        var first = localizer["test"];
        var second = localizer["test"];
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ru");
        var third = localizer["test"];

        // assert — storage consulted exactly once per culture (cache is keyed by culture):
        // the two "en" lookups load once, the "ru" lookup loads once more → 2 total
        first.Is("demo");
        second.Is("demo");
        third.Is("demo");
        storage.LoadCount.Is(2);
    }

    /// <summary>
    /// A locale storage stub that counts how many times LoadLocale is invoked.
    /// </summary>
    private sealed class CountingStorage : ILocaleStorage
    {
        /// <summary>
        /// Number of LoadLocale invocations observed.
        /// </summary>
        public int LoadCount { get; private set; }

        /// <summary>
        /// Records the invocation and returns a fixed locale.
        /// </summary>
        /// <param name="target">The target type to load locale for.</param>
        /// <param name="culture">The culture to load.</param>
        /// <returns>A fixed single-entry locale.</returns>
        public IReadOnlyDictionary<string, string> LoadLocale(Type target, CultureInfo culture)
        {
            LoadCount++;
            return new Dictionary<string, string> { { "test", "demo" } };
        }
    }
}
