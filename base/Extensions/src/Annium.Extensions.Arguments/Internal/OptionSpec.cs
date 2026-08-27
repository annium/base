using System.Collections.Generic;

namespace Annium.Extensions.Arguments.Internal;

/// <summary>
/// What kind of value an option takes.
/// </summary>
internal enum OptionKind
{
    /// <summary>
    /// Takes no value at all.
    /// </summary>
    Flag,

    /// <summary>
    /// Takes exactly one value.
    /// </summary>
    Single,

    /// <summary>
    /// Takes any number of values.
    /// </summary>
    Many,
}

/// <summary>
/// What the lexer needs to know about a configuration type's options in order to read a command line:
/// which spellings mean which option, and what kind of value each takes.
/// </summary>
/// <param name="Names">Normalised spelling - name or alias - to the option's canonical name.</param>
/// <param name="Kinds">Canonical name to the kind of value the option takes.</param>
/// <remarks>
/// None of this is derivable from the shape of the tokens alone. A flag followed by a positional argument
/// looks exactly like an option and its value; two spellings of one option look like two options; and a
/// repeated option is either a list or a mistake depending on the property it binds to.
/// </remarks>
internal sealed record OptionSpec(
    IReadOnlyDictionary<string, string> Names,
    IReadOnlyDictionary<string, OptionKind> Kinds
)
{
    /// <summary>
    /// A spec that knows nothing, for callers parsing without a configuration type in hand.
    /// </summary>
    public static OptionSpec Empty { get; } =
        new(new Dictionary<string, string>(), new Dictionary<string, OptionKind>());

    /// <summary>
    /// Resolves a normalised spelling to the option it names, or to itself when no option claims it.
    /// </summary>
    /// <param name="spelling">The normalised token read from the command line.</param>
    /// <returns>The canonical name of the option.</returns>
    public string Resolve(string spelling) => Names.TryGetValue(spelling, out var name) ? name : spelling;

    /// <summary>
    /// Whether the named option is known to take no value.
    /// </summary>
    /// <param name="name">The canonical name of the option.</param>
    /// <returns>True when the option is a known flag.</returns>
    public bool IsFlag(string name) => Kinds.TryGetValue(name, out var kind) && kind == OptionKind.Flag;

    /// <summary>
    /// Whether the named option is known to take exactly one value. An option this spec knows nothing
    /// about is not one of them: without a configuration type in hand the lexer cannot say, and refusing a
    /// repeat it cannot judge would be worse than passing it on.
    /// </summary>
    /// <param name="name">The canonical name of the option.</param>
    /// <returns>True when the option is known to take a single value.</returns>
    public bool IsSingle(string name) => Kinds.TryGetValue(name, out var kind) && kind == OptionKind.Single;
}
