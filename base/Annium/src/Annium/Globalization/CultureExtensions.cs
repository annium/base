using System.Globalization;

namespace Annium.Globalization;

/// <summary>
/// Provides extension methods for working with culture information.
/// </summary>
public static class CultureExtensions
{
    /// <summary>
    /// Sets the specified culture as the default for the current thread.
    /// </summary>
    /// <param name="culture">The culture to set as default.</param>
    /// <remarks>
    /// This method sets both the current culture and the current UI culture for the default thread.
    /// </remarks>
    public static void SetDefault(this CultureInfo culture)
    {
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
    }
}
