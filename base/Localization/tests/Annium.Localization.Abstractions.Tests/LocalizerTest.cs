using System;
using System.Collections.Generic;
using System.Globalization;
using Annium.Core.DependencyInjection;
using Annium.Localization.Abstractions;
using Annium.Localization.InMemory;
using Annium.Testing;
using Xunit;

namespace Annium.Localization.Abstractions.Tests;

/// <summary>
/// Tests for localization functionality including culture switching, parameter formatting,
/// and culture configuration options.
/// </summary>
public class LocalizerTest : IDisposable
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
    /// Tests basic localization functionality with culture switching.
    /// Verifies that localizer returns correct translations for different cultures.
    /// </summary>
    [Fact]
    public void Localization_Base_Works()
    {
        // arrange
        var localizer = GetLocalizer(_ => { });

        // act
        // invariant culture has no locale → key miss returns the raw key, independent of the ambient culture
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        var iv = localizer["test"];
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en");
        var en = localizer["test"];
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ru");
        var ru = localizer["test"];

        // assert
        iv.Is("test");
        en.Is("demo");
        ru.Is("демо");
    }

    /// <summary>
    /// Tests localization with parameter formatting.
    /// Verifies that localizer correctly formats translated strings with parameters.
    /// </summary>
    [Fact]
    public void Localization_WithParams_Works()
    {
        // arrange
        var localizer = GetLocalizer(_ => { });

        // act
        // invariant culture has no locale → key miss returns the raw key (no-arg indexer)
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        var iv = localizer["test params"];
        // cultures with a "demo {0}" translation exercise the params-formatting path
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en");
        var en = localizer["test params", 5];
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ru");
        var ru = localizer["test params", 5];

        // assert
        iv.Is("test params");
        en.Is("demo 5");
        ru.Is("демо 5");
    }

    /// <summary>
    /// Tests localization with a fixed culture configuration.
    /// Verifies that localizer uses the specified culture regardless of current culture.
    /// </summary>
    [Fact]
    public void Localization_WithSpecifiedCulture_UsesSpecificCulture()
    {
        // arrange
        var localizer = GetLocalizer(opts => opts.UseCulture(CultureInfo.GetCultureInfo("en")));

        // act
        var iv = localizer["test"];
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en");
        var en = localizer["test"];
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ru");
        var ru = localizer["test"];

        // assert
        iv.Is("demo");
        en.Is("demo");
        ru.Is("demo");
    }

    /// <summary>
    /// Tests localization with a culture accessor function.
    /// Verifies that localizer uses the culture accessor to determine the current culture.
    /// </summary>
    [Fact]
    public void Localization_WithSpecifiedCultureAccessor_UsesCultureAccessor()
    {
        // arrange
        var localizer = GetLocalizer(opts => opts.UseCulture(() => CultureInfo.CurrentCulture));

        // act
        // invariant culture has no locale → key miss returns the raw key, independent of the ambient culture
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        var iv = localizer["test"];
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en");
        var en = localizer["test"];
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ru");
        var ru = localizer["test"];

        // assert
        iv.Is("test");
        en.Is("demo");
        ru.Is("демо");
    }

    /// <summary>
    /// Tests that looking up an unknown key in a culture whose locale is loaded returns the raw key.
    /// </summary>
    [Fact]
    public void Translate_KeyMissingInMappedCulture_ReturnsKey()
    {
        // arrange
        var localizer = GetLocalizer(_ => { });

        // act
        // en has a locale loaded, but "nope" is not a registered key → miss returns raw key
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en");
        var result = localizer["nope"];

        // assert
        result.Is("nope");
    }

    /// <summary>
    /// Tests that the IEnumerable&lt;object&gt; overload of the indexer formats the translated string.
    /// </summary>
    [Fact]
    public void Translate_IEnumerableArgs_FormatsCorrectly()
    {
        // arrange
        var localizer = GetLocalizer(_ => { });

        // act
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en");
        var result = localizer["test params", new List<object> { 5 }];

        // assert
        result.Is("demo 5");
    }

    /// <summary>
    /// Tests that format arguments are rendered using the resolved culture as the format provider
    /// (a culture-sensitive decimal separator), not the ambient culture.
    /// </summary>
    [Fact]
    public void Translate_FormatsArgumentUsingResolvedCulture()
    {
        // arrange
        var fr = CultureInfo.GetCultureInfo("fr");
        var container = new ServiceContainer();
        var locales = new Dictionary<CultureInfo, IReadOnlyDictionary<string, string>>
        {
            [fr] = new Dictionary<string, string> { { "val", "x {0}" } },
        };
        // fixed fr culture → string.Format must use fr as the IFormatProvider
        container.AddLocalization(opts => opts.UseInMemoryStorage(locales).UseCulture(fr));
        _provider = container.BuildServiceProvider();
        var localizer = _provider.Resolve<ILocalizer<LocalizerTest>>();

        // act — fr renders 1.5 with a comma separator; proves the resolved culture is used by string.Format
        var result = localizer["val", 1.5m];

        // assert
        result.Is("x 1,5");
    }

    /// <summary>
    /// Creates a configured localizer instance for testing.
    /// </summary>
    /// <param name="configure">Configuration action for localization options</param>
    /// <returns>A configured localizer instance</returns>
    private ILocalizer<LocalizerTest> GetLocalizer(Action<LocalizationOptions> configure)
    {
        var container = new ServiceContainer();

        var locales = new Dictionary<CultureInfo, IReadOnlyDictionary<string, string>>();
        locales[CultureInfo.GetCultureInfo("en")] = new Dictionary<string, string>
        {
            { "test", "demo" },
            { "test params", "demo {0}" },
        };
        locales[CultureInfo.GetCultureInfo("ru")] = new Dictionary<string, string>
        {
            { "test", "демо" },
            { "test params", "демо {0}" },
        };

        container.AddLocalization(opts => configure(opts.UseInMemoryStorage(locales)));

        _provider = container.BuildServiceProvider();

        return _provider.Resolve<ILocalizer<LocalizerTest>>();
    }
}
