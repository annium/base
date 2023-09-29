using System;
using System.Collections.Generic;

namespace Annium.Extensions.Arguments.Internal;

internal sealed record CommandInfo(
    string Id,
    string Description,
    Type Type,
    IReadOnlyCollection<Type> ConfigurationTypes
);