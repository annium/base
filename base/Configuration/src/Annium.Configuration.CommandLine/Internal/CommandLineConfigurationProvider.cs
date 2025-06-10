using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Annium.Configuration.Abstractions;

namespace Annium.Configuration.CommandLine.Internal;

/// <summary>
/// Configuration provider that reads configuration data from command line arguments
/// </summary>
internal class CommandLineConfigurationProvider : ConfigurationProviderBase
{
    /// <summary>
    /// Separator used for hierarchical configuration keys
    /// </summary>
    private const string Separator = "|";

    /// <summary>
    /// Command line arguments to process
    /// </summary>
    private readonly string[] _args;

    /// <summary>
    /// Initializes a new instance of CommandLineConfigurationProvider
    /// </summary>
    /// <param name="args">Command line arguments to process</param>
    public CommandLineConfigurationProvider(string[] args)
    {
        _args = args;
    }

    /// <summary>
    /// Reads configuration data from command line arguments and returns it as a dictionary
    /// </summary>
    /// <returns>Dictionary containing configuration keys and values</returns>
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

    /// <summary>
    /// Determines if a value is a positional argument
    /// </summary>
    /// <param name="value">Value to check</param>
    /// <returns>True if positional argument, false otherwise</returns>
    private bool IsPosition(string value) => !IsOptionLike(value);

    /// <summary>
    /// Determines if a value is an option with a following value
    /// </summary>
    /// <param name="value">Current value to check</param>
    /// <param name="next">Next value in arguments</param>
    /// <returns>True if option with value, false otherwise</returns>
    private bool IsOption(string value, string next) =>
        IsOptionLike(value) && next != string.Empty && !IsOptionLike(next);

    /// <summary>
    /// Determines if a value is a flag without a following value
    /// </summary>
    /// <param name="value">Current value to check</param>
    /// <param name="next">Next value in arguments</param>
    /// <returns>True if flag, false otherwise</returns>
    private bool IsFlag(string value, string next) =>
        IsOptionLike(value) && (next == string.Empty || IsOptionLike(next));

    /// <summary>
    /// Determines if a value looks like an option or flag (starts with -)
    /// </summary>
    /// <param name="value">Value to check</param>
    /// <returns>True if option-like, false otherwise</returns>
    private bool IsOptionLike(string value) => value.StartsWith('-');

    /// <summary>
    /// Parses the name from a command line option or flag
    /// </summary>
    /// <param name="value">Option or flag value to parse</param>
    /// <returns>Parsed configuration key name</returns>
    private string ParseName(string value) =>
        string.Join(
            Separator,
            Regex
                .Replace(value.Trim(), @"^-+", string.Empty)
                .Split('.')
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .Select(e => e.PascalCase())
        );
}
