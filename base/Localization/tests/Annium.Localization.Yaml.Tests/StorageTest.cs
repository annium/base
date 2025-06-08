using System.Globalization;
using Annium.Core.DependencyInjection;
using Annium.Localization.Abstractions;
using Annium.Testing;
using Xunit;

namespace Annium.Localization.Yaml.Tests;

/// <summary>
/// Tests for YAML-based localization storage functionality.
/// Validates locale loading and translation retrieval from YAML files.
/// </summary>
public class StorageTest
{
    /// <summary>
    /// Tests basic localization functionality with YAML storage.
    /// Verifies that localizer correctly retrieves translations from YAML-based locale files.
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
    /// Creates a localizer instance with YAML storage for testing.
    /// </summary>
    /// <returns>A configured localizer instance with YAML storage</returns>
    private ILocalizer<StorageTest> GetLocalizer()
    {
        var container = new ServiceContainer();

        container.AddLocalization(opts => opts.UseYamlStorage());

        return container.BuildServiceProvider().Resolve<ILocalizer<StorageTest>>();
    }
}
