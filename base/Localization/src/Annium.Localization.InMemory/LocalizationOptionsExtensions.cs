using System.Collections.Generic;
using System.Globalization;
using Annium.Localization.Abstractions;
using Annium.Localization.InMemory;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class LocalizationOptionsExtensions
{
    public static LocalizationOptions UseInMemoryStorage(
        this LocalizationOptions options
    )
    {
        return options.UseInMemoryStorage(new Dictionary<CultureInfo, IReadOnlyDictionary<string, string>>());
    }

    public static LocalizationOptions UseInMemoryStorage(
        this LocalizationOptions options,
        IReadOnlyDictionary<CultureInfo, IReadOnlyDictionary<string, string>> locales
    )
    {
        options.SetLocaleStorage(container =>
        {
            var storage = new Storage(locales);
            container.Add<ILocaleStorage>(storage).AsSelf().Singleton();
        });

        return options;
    }
}