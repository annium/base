using System;
using System.Collections.Generic;

namespace Annium.Extensions.Arguments.Internal;

internal interface IHelpBuilder
{
    string BuildHelp(string id, string description, IReadOnlyCollection<CommandInfo> commands);

    string BuildHelp(string id, string description, params Type[] configurationTypes);
}