using System.Globalization;
using Annium.Globalization;
using Annium.Testing;
using Xunit;

namespace Annium.Tests.Globalization;

/// <summary>
/// Tests for <see cref="CultureExtensions"/>. Closes the TG9 zero-coverage gap.
/// </summary>
public class CultureExtensionsTests
{
    /// <summary>
    /// Verifies that <see cref="CultureExtensions.SetDefault"/> updates BOTH the default thread
    /// culture and the default thread UI culture in the same call. A regression that sets only one
    /// of the two static properties would silently break formatting OR localization across the
    /// process — this test catches either omission.
    /// </summary>
    [Fact]
    public void SetDefault_SetsBothCurrentCultureAndUICulture()
    {
        // arrange — capture originals so we can restore (process-global side effect).
        var originalCulture = CultureInfo.DefaultThreadCurrentCulture;
        var originalUiCulture = CultureInfo.DefaultThreadCurrentUICulture;
        try
        {
            // act
            var target = CultureInfo.GetCultureInfo("fr-FR");
            target.SetDefault();

            // assert
            CultureInfo.DefaultThreadCurrentCulture.Is(target);
            CultureInfo.DefaultThreadCurrentUICulture.Is(target);
        }
        finally
        {
            CultureInfo.DefaultThreadCurrentCulture = originalCulture;
            CultureInfo.DefaultThreadCurrentUICulture = originalUiCulture;
        }
    }
}
