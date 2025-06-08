using System.Collections.Generic;
using System.Globalization;
using Annium.Core.DependencyInjection;
using Annium.Localization.Abstractions;
using Annium.Testing;
using Xunit;

namespace Annium.Localization.InMemory.Tests;

/// <summary>
/// Tests for in-memory localization storage functionality.
/// Validates locale loading and translation retrieval from memory-based storage.
/// </summary>
public class StorageTest
{
    /// <summary>
    /// Tests basic localization functionality with in-memory storage.
    /// Verifies that localizer correctly retrieves translations from memory-based locale storage.
    /// </summary>
    [Fact]
    public void Localization_Works()
    {
        // arrange
        var localizer = GetLocalizer();

        // act
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en");
        var en = localizer["test"];
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ru");
        var ru = localizer["test"];

        // assert
        en.Is("demo");
        ru.Is("демо");
    }

    /// <summary>
    /// Creates a localizer instance with in-memory storage for testing.
    /// </summary>
    /// <returns>A configured localizer instance with in-memory storage</returns>
    private ILocalizer<StorageTest> GetLocalizer()
    {
        var container = new ServiceContainer();

        var locales = new Dictionary<CultureInfo, IReadOnlyDictionary<string, string>>();
        locales[CultureInfo.GetCultureInfo("en")] = new Dictionary<string, string> { { "test", "demo" } };
        locales[CultureInfo.GetCultureInfo("ru")] = new Dictionary<string, string> { { "test", "демо" } };

        container.AddLocalization(opts => opts.UseInMemoryStorage(locales));

        return container.BuildServiceProvider().Resolve<ILocalizer<StorageTest>>();
    }
}
