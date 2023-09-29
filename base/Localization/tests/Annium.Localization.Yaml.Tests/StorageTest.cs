using System.Globalization;
using Annium.Core.DependencyInjection;
using Annium.Localization.Abstractions;
using Annium.Testing;
using Xunit;

namespace Annium.Localization.Yaml.Tests;

public class StorageTest
{
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

    private ILocalizer<StorageTest> GetLocalizer()
    {
        var container = new ServiceContainer();

        container.AddLocalization(opts => opts.UseYamlStorage());

        return container.BuildServiceProvider().Resolve<ILocalizer<StorageTest>>();
    }
}