using System;
using NodaTime;

namespace Annium.Extensions.Jobs.Internal;

internal interface IIntervalResolver
{
    Func<Instant, bool> GetMatcher(string interval);
}