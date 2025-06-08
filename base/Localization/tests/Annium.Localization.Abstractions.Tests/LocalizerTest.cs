using System;
using System.Collections.Generic;
using System.Globalization;
using Annium.Core.DependencyInjection;
using Annium.Testing;
using Xunit;

namespace Annium.Localization.Abstractions.Tests;

/// <summary>
/// Tests for localization functionality including culture switching, parameter formatting,
/// and culture configuration options.
/// </summary>
public class LocalizerTest
{
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
        var iv = localizer["test params", 5];
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

        return container.BuildServiceProvider().Resolve<ILocalizer<LocalizerTest>>();
    }
}
