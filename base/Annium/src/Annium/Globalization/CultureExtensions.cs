using System.Globalization;

namespace Annium.Globalization;

public static class CultureExtensions
{
    public static void SetDefault(this CultureInfo culture)
    {
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
    }
}