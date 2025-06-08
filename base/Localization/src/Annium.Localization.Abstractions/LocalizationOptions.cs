using System;
using System.Globalization;
using Annium.Core.DependencyInjection;

namespace Annium.Localization.Abstractions;

/// <summary>
/// Configuration options for localization services
/// </summary>
public class LocalizationOptions
{
    /// <summary>
    /// Service container for locale storage services
    /// </summary>
    internal IServiceContainer LocaleStorageServices { get; private set; } = new ServiceContainer();

    /// <summary>
    /// Function to access the current culture
    /// </summary>
    internal Func<CultureInfo> CultureAccessor { get; private set; } = () => CultureInfo.CurrentCulture;

    /// <summary>
    /// Initializes a new instance of the LocalizationOptions class
    /// </summary>
    internal LocalizationOptions() { }

    /// <summary>
    /// Configures the locale storage services
    /// </summary>
    /// <param name="configure">The configuration action</param>
    /// <returns>The options instance for method chaining</returns>
    public LocalizationOptions SetLocaleStorage(Action<IServiceContainer> configure)
    {
        configure(LocaleStorageServices = new ServiceContainer());

        return this;
    }

    /// <summary>
    /// Sets a fixed culture to use for localization
    /// </summary>
    /// <param name="culture">The culture to use</param>
    /// <returns>The options instance for method chaining</returns>
    public LocalizationOptions UseCulture(CultureInfo culture)
    {
        CultureAccessor = () => culture;

        return this;
    }

    /// <summary>
    /// Sets a culture accessor function for dynamic culture resolution
    /// </summary>
    /// <param name="accessor">The culture accessor function</param>
    /// <returns>The options instance for method chaining</returns>
    public LocalizationOptions UseCulture(Func<CultureInfo> accessor)
    {
        CultureAccessor = accessor;

        return this;
    }
}
