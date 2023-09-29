using System;
using System.Globalization;
using Annium.Core.DependencyInjection;

namespace Annium.Localization.Abstractions;

public class LocalizationOptions
{
    internal IServiceContainer LocaleStorageServices { get; private set; } = new ServiceContainer();
    internal Func<CultureInfo> CultureAccessor { get; private set; } = () => CultureInfo.CurrentCulture;

    internal LocalizationOptions()
    {
    }

    public LocalizationOptions SetLocaleStorage(Action<IServiceContainer> configure)
    {
        configure(LocaleStorageServices = new ServiceContainer());

        return this;
    }

    public LocalizationOptions UseCulture(CultureInfo culture)
    {
        CultureAccessor = () => culture;

        return this;
    }

    public LocalizationOptions UseCulture(Func<CultureInfo> accessor)
    {
        CultureAccessor = accessor;

        return this;
    }
}