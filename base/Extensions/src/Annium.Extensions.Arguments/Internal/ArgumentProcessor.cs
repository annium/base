using System;
using System.Collections.Generic;
using System.Linq;

namespace Annium.Extensions.Arguments.Internal;

internal class ArgumentProcessor : IArgumentProcessor
{
    public RawConfiguration Compose(string[] args)
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

            var name = value.PascalCase();
            var next = i < args.Length - 1 ? args[i + 1] : string.Empty;

            if (IsOption(value, next))
            {
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

            else if (IsFlag(value, next))
                if (flags.Contains(name))
                    throw new Exception($"Same flag '{value}' is used twice");
                else
                    flags.Add(name);

            else
                throw new Exception($"Can't process value '{value}', followed by '{next}'");
        }

        return new RawConfiguration(
            positions,
            flags,
            options,
            multiOptions.ToDictionary(
                e => e.Key,
                e => e.Value.ToArray() as IReadOnlyCollection<string>
            ),
            raw
        );
    }

    private bool IsPosition(string value) =>
        !IsOptionLike(value);

    private bool IsOption(string value, string next) =>
        IsOptionLike(value) && !string.IsNullOrEmpty(next) && !IsOptionLike(next);

    private bool IsFlag(string value, string next) =>
        IsOptionLike(value) && (string.IsNullOrEmpty(next) || IsOptionLike(next));

    private bool IsRawDelimeter(string value) =>
        value.All(c => c == Constants.OptionSign);

    private bool IsOptionLike(string value) =>
        value.StartsWith(Constants.OptionSign);
}