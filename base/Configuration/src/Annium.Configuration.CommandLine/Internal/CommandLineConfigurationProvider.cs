using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Annium.Configuration.Abstractions;

namespace Annium.Configuration.CommandLine.Internal;

internal class CommandLineConfigurationProvider : ConfigurationProviderBase
{
    private const string Separator = "|";

    private readonly string[] _args;

    public CommandLineConfigurationProvider(string[] args)
    {
        _args = args;
    }

    public override IReadOnlyDictionary<string[], string> Read()
    {
        Init();

        var flags = new List<string>();
        var options = new Dictionary<string, string>();
        var multiOptions = new Dictionary<string, List<string>>();

        for (var i = 0; i < _args.Length; i++)
        {
            var value = _args[i];
            if (IsPosition(value))
                continue;

            var name = ParseName(value);
            var next = i < _args.Length - 1 ? _args[i + 1] : string.Empty;

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

        foreach (var name in flags)
            Data[name.Split(Separator)] = true.ToString();

        foreach (var (name, value) in options)
            Data[name.Split(Separator)] = value;

        foreach (var (name, values) in multiOptions)
        {
            var path = name.Split(Separator);
            for (var i = 0; i < values.Count; i++)
                Data[path.Append(i.ToString()).ToArray()] = values[i];
        }

        return Data;
    }

    private bool IsPosition(string value) =>
        !IsOptionLike(value);

    private bool IsOption(string value, string next) =>
        IsOptionLike(value) && next != string.Empty && !IsOptionLike(next);

    private bool IsFlag(string value, string next) =>
        IsOptionLike(value) && (next == string.Empty || IsOptionLike(next));

    private bool IsOptionLike(string value) =>
        value.StartsWith('-');

    private string ParseName(string value) => string.Join(Separator,
        Regex.Replace(value.Trim(), @"^-+", string.Empty)
            .Split('.')
            .Where(e => !string.IsNullOrWhiteSpace(e))
            .Select(e => e.PascalCase())
    );
}