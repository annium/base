using System.Collections.Generic;

namespace Annium.Configuration.Abstractions;

public interface IConfigurationContainer
{
    IConfigurationContainer Add(IReadOnlyDictionary<string[], string> config);
    IReadOnlyDictionary<string[], string> Get();
}