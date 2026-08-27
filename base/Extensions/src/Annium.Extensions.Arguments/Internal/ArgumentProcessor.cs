using System;
using System.Collections.Generic;
using System.Linq;

namespace Annium.Extensions.Arguments.Internal;

/// <summary>
/// Processes command line arguments and converts them into a structured raw configuration.
/// Handles positional arguments, flags, options with values, multi-value options, and raw arguments.
/// </summary>
internal class ArgumentProcessor : IArgumentProcessor
{
    /// <summary>
    /// Composes a raw configuration from command line arguments by parsing positional arguments,
    /// flags, options, multi-options, and raw content after delimiter.
    /// </summary>
    /// <param name="args">Array of command line arguments to process</param>
    /// <param name="spec">What the configuration type's options are - see <see cref="OptionSpec"/></param>
    /// <returns>A structured raw configuration containing parsed argument data</returns>
    public RawConfiguration Compose(string[] args, OptionSpec spec)
    {
        var positions = new List<string>();
        var flags = new List<string>();
        var options = new Dictionary<string, string>();
        var multiOptions = new Dictionary<string, List<string>>();
        var raw = string.Empty;

        for (var i = 0; i < args.Length; i++)
        {
            var value = args[i];
            if (IsPosition(value))
            {
                positions.Add(value);
                continue;
            }

            if (IsRawDelimeter(value))
            {
                raw = string.Join(' ', args.Skip(i + 1));
                break;
            }

            // resolved to the option it names, so that two spellings of one option are one option rather
            // than two entries the binder reads only one of
            var name = spec.Resolve(value.PascalCase());
            var next = i < args.Length - 1 ? args[i + 1] : string.Empty;

            // a flag never takes a value, so the token after it is somebody else's - reading it as this
            // option's value silently loses the flag and the argument at once
            var isKnownFlag = spec.IsFlag(name);

            if (!isKnownFlag && IsOption(value, next))
            {
                var isRepeat = multiOptions.ContainsKey(name) || options.ContainsKey(name);

                // only an option that binds to a collection can hold more than one value; for any other,
                // a repeat used to be promoted into a bucket the binder never reads, losing both values
                if (isRepeat && spec.IsSingle(name))
                    throw new ArgumentParseException($"Option '{name}' is given more than once");

                if (multiOptions.ContainsKey(name))
                    multiOptions[name].Add(next);
                else if (options.ContainsKey(name))
                {
                    multiOptions[name] = new List<string> { options[name], next };
                    options.Remove(name);
                }
                else
                    options[name] = next;

                i++;
            }
            else if (isKnownFlag || IsFlag(value, next))
                if (flags.Contains(name))
                    throw new ArgumentParseException($"Same flag '{value}' is used twice");
                else
                    flags.Add(name);
            else
                throw new ArgumentParseException($"Can't process value '{value}', followed by '{next}'");
        }

        return new RawConfiguration(
            positions,
            flags,
            options,
            multiOptions.ToDictionary(e => e.Key, e => e.Value.ToArray() as IReadOnlyCollection<string>),
            raw
        );
    }

    /// <summary>
    /// Determines if the given value is a positional argument (not an option or flag).
    /// </summary>
    /// <param name="value">The argument value to check</param>
    /// <returns>True if the value is a positional argument, false otherwise</returns>
    private bool IsPosition(string value) => !IsOptionLike(value);

    /// <summary>
    /// Determines if the given value is an option with a following value.
    /// </summary>
    /// <param name="value">The current argument value to check</param>
    /// <param name="next">The next argument value in sequence</param>
    /// <returns>True if the value is an option with a value, false otherwise</returns>
    private bool IsOption(string value, string next) =>
        IsOptionLike(value) && !string.IsNullOrEmpty(next) && !IsOptionLike(next);

    /// <summary>
    /// Determines if the given value is a flag (option without a value).
    /// </summary>
    /// <param name="value">The current argument value to check</param>
    /// <param name="next">The next argument value in sequence</param>
    /// <returns>True if the value is a flag, false otherwise</returns>
    private bool IsFlag(string value, string next) =>
        IsOptionLike(value) && (string.IsNullOrEmpty(next) || IsOptionLike(next));

    /// <summary>
    /// Determines if the given value is a raw delimiter (consists only of option signs).
    /// </summary>
    /// <param name="value">The argument value to check</param>
    /// <returns>True if the value is a raw delimiter, false otherwise</returns>
    private bool IsRawDelimeter(string value) => value.All(c => c == Constants.OptionSign);

    /// <summary>
    /// Determines if the given value looks like an option or flag (starts with option sign).
    /// </summary>
    /// <param name="value">The argument value to check</param>
    /// <returns>True if the value starts with an option sign, false otherwise</returns>
    private bool IsOptionLike(string value) => value.StartsWith(Constants.OptionSign) && !IsNegativeNumber(value);

    /// <summary>
    /// Determines whether the value is a negative number rather than an option. A leading option sign
    /// followed by a digit is arithmetic, not a name: reading it as an option made both an option's
    /// negative value and a negative positional argument disappear.
    /// </summary>
    /// <param name="value">The argument value to check</param>
    /// <returns>True if the value reads as a negative number, false otherwise</returns>
    private bool IsNegativeNumber(string value) => value.Length > 1 && char.IsAsciiDigit(value[1]);
}
