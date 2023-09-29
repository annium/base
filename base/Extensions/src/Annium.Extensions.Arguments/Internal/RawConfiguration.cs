using System.Collections.Generic;

namespace Annium.Extensions.Arguments.Internal;

internal class RawConfiguration
{
    public IReadOnlyCollection<string> Positions { get; }

    public IReadOnlyCollection<string> Flags { get; }

    public IReadOnlyDictionary<string, string> Options { get; }

    public IReadOnlyDictionary<string, IReadOnlyCollection<string>> MultiOptions { get; }

    public string Raw { get; }

    public RawConfiguration(
        IReadOnlyCollection<string> positions,
        IReadOnlyCollection<string> flags,
        IReadOnlyDictionary<string, string> options,
        IReadOnlyDictionary<string, IReadOnlyCollection<string>> multiOptions,
        string raw
    )
    {
        Positions = positions;
        Flags = flags;
        Options = options;
        MultiOptions = multiOptions;
        Raw = raw;
    }
}