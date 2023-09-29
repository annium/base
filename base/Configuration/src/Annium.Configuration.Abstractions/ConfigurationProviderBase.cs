using System.Collections.Generic;
using System.Linq;

namespace Annium.Configuration.Abstractions;

public abstract class ConfigurationProviderBase
{
    protected Dictionary<string[], string> Data = new();

    protected Stack<string> Context = new();

    protected string[] Path => Context.Reverse().ToArray();

    public abstract IReadOnlyDictionary<string[], string> Read();

    protected void Init()
    {
        Data = new Dictionary<string[], string>();
        Context = new Stack<string>();
    }
}