using System;
using System.Collections.Generic;
using System.Globalization;
using Annium.Core.DependencyInjection;
using Annium.Localization.Abstractions;
using Annium.Testing;
using Xunit;

namespace Annium.Localization.InMemory.Tests;

/// <summary>
/// Tests for the per-type, shared, and combined in-memory locale storage lookup behaviour.
/// </summary>
public class StorageBehaviorTests : IDisposable
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
    /// Verifies that a type-specific locale entry is returned when the localizer type has its own
    /// per-type map entry for the current culture.
    /// </summary>
    [Fact]
    public void LoadLocale_TypeSpecificLocale_ReturnsTypeEntry()
    {
        // arrange
        var en = CultureInfo.GetCultureInfo("en");
        var localesByType = new Dictionary<Type, IReadOnlyDictionary<CultureInfo, IReadOnlyDictionary<string, string>>>
        {
            [typeof(StorageBehaviorTests)] = new Dictionary<CultureInfo, IReadOnlyDictionary<string, string>>
            {
                [en] = new Dictionary<string, string> { { "test", "typed" } },
            },
        };

        var localizer = GetLocalizer(opts => opts.UseInMemoryStorage(localesByType));

        // act
        CultureInfo.CurrentCulture = en;
        var result = localizer["test"];

        // assert
        result.Is("typed");
    }

    /// <summary>
    /// Verifies that the shared locale is used as a fallback when the test type has no per-type
    /// entry in the locales-by-type map.
    /// </summary>
    [Fact]
    public void LoadLocale_TypeWithoutSpecificLocale_FallsBackToShared()
    {
        // arrange
        var en = CultureInfo.GetCultureInfo("en");

        // per-type map covers a DIFFERENT type — not StorageBehaviorTests
        var localesByType = new Dictionary<Type, IReadOnlyDictionary<CultureInfo, IReadOnlyDictionary<string, string>>>
        {
            [typeof(StorageTest)] = new Dictionary<CultureInfo, IReadOnlyDictionary<string, string>>
            {
                [en] = new Dictionary<string, string> { { "test", "other-typed" } },
            },
        };

        var sharedLocales = new Dictionary<CultureInfo, IReadOnlyDictionary<string, string>>
        {
            [en] = new Dictionary<string, string> { { "test", "shared" } },
        };

        var localizer = GetLocalizer(opts => opts.UseInMemoryStorage(localesByType, sharedLocales));

        // act
        CultureInfo.CurrentCulture = en;
        var result = localizer["test"];

        // assert
        result.Is("shared");
    }

    /// <summary>
    /// Verifies that a type-specific entry wins over the shared entry when both exist for the same
    /// key and culture in the combined overload.
    /// </summary>
    [Fact]
    public void LoadLocale_CombinedOverload_TypeWinsOverShared()
    {
        // arrange
        var en = CultureInfo.GetCultureInfo("en");

        var localesByType = new Dictionary<Type, IReadOnlyDictionary<CultureInfo, IReadOnlyDictionary<string, string>>>
        {
            [typeof(StorageBehaviorTests)] = new Dictionary<CultureInfo, IReadOnlyDictionary<string, string>>
            {
                [en] = new Dictionary<string, string> { { "test", "typed" } },
            },
        };

        var sharedLocales = new Dictionary<CultureInfo, IReadOnlyDictionary<string, string>>
        {
            [en] = new Dictionary<string, string> { { "test", "shared" } },
        };

        var localizer = GetLocalizer(opts => opts.UseInMemoryStorage(localesByType, sharedLocales));

        // act
        CultureInfo.CurrentCulture = en;
        var result = localizer["test"];

        // assert
        result.Is("typed");
    }

    /// <summary>
    /// Verifies that when the type has a per-type map but the current culture is absent from it,
    /// the lookup falls through (to shared/empty) rather than returning another culture's entry.
    /// </summary>
    [Fact]
    public void LoadLocale_TypeSpecificButCultureMissing_ReturnsKey()
    {
        // arrange
        var en = CultureInfo.GetCultureInfo("en");
        var localesByType = new Dictionary<Type, IReadOnlyDictionary<CultureInfo, IReadOnlyDictionary<string, string>>>
        {
            [typeof(StorageBehaviorTests)] = new Dictionary<CultureInfo, IReadOnlyDictionary<string, string>>
            {
                [en] = new Dictionary<string, string> { { "test", "typed" } },
            },
        };

        var localizer = GetLocalizer(opts => opts.UseInMemoryStorage(localesByType));

        // act
        // the type IS registered, but "ru" is absent from its sub-map and there is no shared locale → miss
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ru");
        var result = localizer["test"];

        // assert
        result.Is("test");
    }

    /// <summary>
    /// Verifies that when the type has a per-type map lacking the current culture, but the shared map
    /// HAS that culture, the shared entry is returned (the per-type/shared fall-through, with shared present).
    /// </summary>
    [Fact]
    public void LoadLocale_TypePresentButCultureMissing_FallsBackToShared()
    {
        // arrange
        var en = CultureInfo.GetCultureInfo("en");
        var ru = CultureInfo.GetCultureInfo("ru");

        // the type IS registered, but only for en — ru is absent from its sub-map
        var localesByType = new Dictionary<Type, IReadOnlyDictionary<CultureInfo, IReadOnlyDictionary<string, string>>>
        {
            [typeof(StorageBehaviorTests)] = new Dictionary<CultureInfo, IReadOnlyDictionary<string, string>>
            {
                [en] = new Dictionary<string, string> { { "test", "typed" } },
            },
        };

        // shared HAS ru
        var sharedLocales = new Dictionary<CultureInfo, IReadOnlyDictionary<string, string>>
        {
            [ru] = new Dictionary<string, string> { { "test", "shared-ru" } },
        };

        var localizer = GetLocalizer(opts => opts.UseInMemoryStorage(localesByType, sharedLocales));

        // act
        CultureInfo.CurrentCulture = ru;
        var result = localizer["test"];

        // assert
        result.Is("shared-ru");
    }

    /// <summary>
    /// Verifies that an empty in-memory storage (no-arg overload) returns the raw key for any lookup.
    /// </summary>
    [Fact]
    public void LoadLocale_EmptyStorage_ReturnsKey()
    {
        // arrange
        var localizer = GetLocalizer(opts => opts.UseInMemoryStorage());

        // invariant culture has no locale → miss returns raw key
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        // act
        var result = localizer["anything"];

        // assert
        result.Is("anything");
    }

    /// <summary>
    /// Builds a localizer for <see cref="StorageBehaviorTests"/> using the provided configure action.
    /// </summary>
    /// <param name="configure">Configuration action for localization options.</param>
    /// <returns>A configured <see cref="ILocalizer{T}"/> instance.</returns>
    private ILocalizer<StorageBehaviorTests> GetLocalizer(Action<LocalizationOptions> configure)
    {
        var container = new ServiceContainer();
        container.AddLocalization(configure);
        _provider = container.BuildServiceProvider();
        return _provider.Resolve<ILocalizer<StorageBehaviorTests>>();
    }
}
