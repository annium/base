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
    /// Gets whether a locale storage backend has been configured via <see cref="SetLocaleStorage"/>.
    /// Used by <c>AddLocalization</c> to fail fast when no storage was registered.
    /// </summary>
    internal bool IsStorageConfigured { get; private set; }

    /// <summary>
    /// Initializes a new instance of the LocalizationOptions class
    /// </summary>
    internal LocalizationOptions() { }

    /// <summary>
    /// Configures the locale storage services. A single storage backend is supported per
    /// options instance; calling this more than once throws to surface the misconfiguration
    /// rather than silently discarding the earlier storage.
    /// </summary>
    /// <param name="configure">The configuration action</param>
    /// <returns>The options instance for method chaining</returns>
    /// <exception cref="InvalidOperationException">Thrown if locale storage was already configured.</exception>
    public LocalizationOptions SetLocaleStorage(Action<IServiceContainer> configure)
    {
        if (IsStorageConfigured)
            throw new InvalidOperationException(
                "Locale storage is already configured; a single storage backend is supported per LocalizationOptions."
            );

        configure(LocaleStorageServices = new ServiceContainer());

        // set only after configure succeeds, so a throwing configure does not poison the flag
        // (a retry must still be able to configure storage, and AddLocalization's guard must not pass)
        IsStorageConfigured = true;

        return this;
    }

    /// <summary>
    /// Detaches the staging locale-storage container after its services have been transferred to the
    /// host container, so a retained options reference cannot observe the populated registrations.
    /// </summary>
    internal void DetachLocaleStorage() => LocaleStorageServices = new ServiceContainer();

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
