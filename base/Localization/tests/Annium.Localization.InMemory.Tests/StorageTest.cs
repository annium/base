using System.Collections.Generic;
using System.Globalization;
using Annium.Core.DependencyInjection;
using Annium.Localization.Abstractions;
using Annium.Testing;
using Xunit;

namespace Annium.Localization.InMemory.Tests;

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

        var locales = new Dictionary<CultureInfo, IReadOnlyDictionary<string, string>>();
        locales[CultureInfo.GetCultureInfo("en")] = new Dictionary<string, string> { { "test", "demo" } };
        locales[CultureInfo.GetCultureInfo("ru")] = new Dictionary<string, string> { { "test", "демо" } };

        container.AddLocalization(opts => opts.UseInMemoryStorage(locales));

        return container.BuildServiceProvider().Resolve<ILocalizer<StorageTest>>();
    }
}