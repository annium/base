using System;
using Annium.Extensions.Arguments.Internal;

namespace Annium.Extensions.Arguments;

/// <summary>
/// Internal root record containing the core dependencies for argument processing
/// </summary>
/// <param name="Provider">The service provider instance</param>
/// <param name="ConfigurationBuilder">The configuration builder instance</param>
/// <param name="HelpBuilder">The help builder instance</param>
internal sealed record Root(
    IServiceProvider Provider,
    IConfigurationBuilder ConfigurationBuilder,
    IHelpBuilder HelpBuilder
);
