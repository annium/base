using System;
using Annium.Extensions.Arguments.Internal;

namespace Annium.Extensions.Arguments;

internal sealed record Root(
    IServiceProvider Provider,
    IConfigurationBuilder ConfigurationBuilder,
    IHelpBuilder HelpBuilder
);