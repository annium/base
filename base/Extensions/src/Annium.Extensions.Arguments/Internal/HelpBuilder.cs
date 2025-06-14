using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Annium.Extensions.Arguments.Internal;

/// <summary>
/// Builds formatted help text for commands and their arguments by analyzing configuration types
/// and generating usage strings, descriptions, and parameter documentation.
/// </summary>
internal class HelpBuilder : IHelpBuilder
{
    /// <summary>
    /// Processes configuration types to extract property metadata for help generation.
    /// </summary>
    private readonly IConfigurationProcessor _configurationProcessor;

    /// <summary>
    /// Initializes a new instance of the HelpBuilder class with the required configuration processor.
    /// </summary>
    /// <param name="configurationProcessor">The processor for extracting property metadata from configuration types</param>
    public HelpBuilder(IConfigurationProcessor configurationProcessor)
    {
        _configurationProcessor = configurationProcessor;
    }

    /// <summary>
    /// Builds help text for a command-based application showing available commands and their descriptions.
    /// </summary>
    /// <param name="id">The application identifier</param>
    /// <param name="description">The application description</param>
    /// <param name="commands">Collection of available commands</param>
    /// <returns>Formatted help text showing usage and available commands</returns>
    public string BuildHelp(string id, string description, IReadOnlyCollection<CommandInfo> commands)
    {
        var sb = new StringBuilder();

        // usage string
        sb.AppendLine();
        sb.AppendLine($"Usage: {id} <command> [...arguments]");

        // description
        sb.AppendLine();
        sb.AppendLine(description);

        // commands
        sb.AppendLine();
        sb.AppendLine("Commands:");
        var maxCommandlength = commands.Max(e => e.Id.Length);
        foreach (var cmd in commands.OrderBy(e => e.Id))
            sb.AppendLine($"  {cmd.Id.PadRight(maxCommandlength)}  {cmd.Description}");

        return sb.ToString();
    }

    /// <summary>
    /// Builds help text for a single command showing usage, positions, options, and their descriptions.
    /// </summary>
    /// <param name="id">The command identifier</param>
    /// <param name="description">The command description</param>
    /// <param name="configurationTypes">Array of configuration types to analyze for argument documentation</param>
    /// <returns>Formatted help text showing usage, positions, and options</returns>
    public string BuildHelp(string id, string description, params Type[] configurationTypes)
    {
        var sb = new StringBuilder();

        var positions = GetPositions(configurationTypes);
        var options = GetOptions(configurationTypes);
        var helps = GetHelps(configurationTypes);
        var raw = GetRaw(configurationTypes);

        // usage string
        sb.AppendLine();
        sb.Append($"Usage: {id}".Trim());
        if (positions.Count > 0)
            foreach (var (position, isRequired) in positions)
                sb.Append($" {Usage(position, isRequired)}");
        if (options.Count > 0)
            foreach (var (option, (optionAlias, isRequired, type)) in options)
            {
                var optionView = optionAlias is null ? Option(option) : $"{Option(optionAlias)}|{Option(option)}";
                var optionUsage = Usage(option, true);

                switch (type)
                {
                    case OptionType.Normal:
                        sb.Append(isRequired ? $" {optionView} {optionUsage}" : $" [{optionView} {optionUsage}]");
                        break;
                    case OptionType.Multi:
                        sb.Append(isRequired ? $" ({optionView} {optionUsage})[]" : $" [{optionView} {optionUsage}][]");
                        break;
                    case OptionType.Flag:
                        sb.Append($" [{optionView}]");
                        break;
                }
            }

        if (raw != null)
            sb.Append($" [...{raw}]");
        sb.AppendLine();

        // description
        sb.AppendLine();
        sb.AppendLine(description);

        // positions
        if (positions.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("Positions:");
            var descriptions = positions.ToDictionary(p => p.Key, p => Description(p.Key, p.Value));
            var maxPositionlength = descriptions.Values.Max(d => d.Length);
            foreach (var position in positions.Keys)
            {
                var name = descriptions[position].PadRight(maxPositionlength);
                var help = helps.ContainsKey(position) ? helps[position] : string.Empty;
                sb.AppendLine($"  {name}  {help}".TrimEnd());
            }
        }

        // options
        if (options.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("Options:");
            var aliases = options.ToDictionary(
                o => o.Key,
                o => o.Value.Item1 == null ? string.Empty : Option(o.Value.Item1) + ','
            );
            var maxAliasLength = aliases.Values.Max(e => e.Length);
            var descriptions = options.ToDictionary(
                o => o.Key,
                o =>
                    Description(
                        Option(o.Key) + (o.Value.Item3 == OptionType.Multi ? "[]" : string.Empty),
                        o.Value.Item2
                    )
            );
            var maxDescriptionLength = descriptions.Values.Max(e => e.Length);
            foreach (var option in options.Keys)
            {
                var alias = aliases[option].PadRight(maxAliasLength);
                var name = descriptions[option].PadRight(maxDescriptionLength);
                var help = helps.ContainsKey(option) ? helps[option] : string.Empty;
                sb.AppendLine($"  {alias} {name}  {help}".TrimEnd());
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Extracts positional arguments from configuration types and their required status.
    /// </summary>
    /// <param name="types">Array of configuration types to analyze</param>
    /// <returns>Dictionary mapping position names to their required status</returns>
    private IReadOnlyDictionary<string, bool> GetPositions(Type[] types) =>
        types
            .SelectMany(t => _configurationProcessor.GetPropertiesWithAttribute<PositionAttribute>(t))
            .OrderBy(e => e.attribute.Position)
            .ToDictionary(e => e.property.Name.KebabCase(), e => e.attribute.IsRequired);

    /// <summary>
    /// Extracts option arguments from configuration types including aliases, required status, and option type.
    /// </summary>
    /// <param name="types">Array of configuration types to analyze</param>
    /// <returns>Dictionary mapping option names to their alias, required status, and option type</returns>
    private IReadOnlyDictionary<string, ValueTuple<string?, bool, OptionType>> GetOptions(Type[] types) =>
        types
            .SelectMany(t => _configurationProcessor.GetPropertiesWithAttribute<OptionAttribute>(t))
            .OrderBy(e => e.attribute.IsRequired)
            .ThenBy(e => e.property.Name)
            .ToDictionary(
                e => e.property.Name.KebabCase(),
                e =>
                    (
                        e.attribute.Alias?.KebabCase(),
                        e.attribute.IsRequired,
                        e.property.PropertyType.IsArray ? OptionType.Multi
                        : e.property.PropertyType == typeof(bool) ? OptionType.Flag
                        : OptionType.Normal
                    )
            );

    /// <summary>
    /// Extracts help text for arguments from configuration types.
    /// </summary>
    /// <param name="types">Array of configuration types to analyze</param>
    /// <returns>Dictionary mapping argument names to their help text</returns>
    private IReadOnlyDictionary<string, string> GetHelps(Type[] types) =>
        types
            .SelectMany(t => _configurationProcessor.GetPropertiesWithAttribute<HelpAttribute>(t))
            .ToDictionary(e => e.property.Name.KebabCase(), e => e.attribute.Help);

    /// <summary>
    /// Finds the raw argument property name from configuration types.
    /// </summary>
    /// <param name="types">Array of configuration types to analyze</param>
    /// <returns>The name of the raw argument property if found, null otherwise</returns>
    private string? GetRaw(Type[] types) =>
        types
            .SelectMany(t => _configurationProcessor.GetPropertiesWithAttribute<RawAttribute>(t))
            .Select(e => e.property.Name.KebabCase())
            .FirstOrDefault();

    /// <summary>
    /// Formats an argument name as a command line option by prefixing with the option sign.
    /// </summary>
    /// <param name="argument">The argument name to format</param>
    /// <returns>The formatted option string</returns>
    private string Option(string argument) => $"{Constants.OptionSign}{argument}";

    /// <summary>
    /// Formats an argument for usage display, showing required arguments in angle brackets and optional in square brackets.
    /// </summary>
    /// <param name="argument">The argument name to format</param>
    /// <param name="isRequired">Whether the argument is required</param>
    /// <returns>The formatted usage string</returns>
    private string Usage(string argument, bool isRequired) => isRequired ? $"<{argument}>" : $"[{argument}]";

    /// <summary>
    /// Formats an argument for description display, appending a question mark for optional arguments.
    /// </summary>
    /// <param name="argument">The argument name to format</param>
    /// <param name="isRequired">Whether the argument is required</param>
    /// <returns>The formatted description string</returns>
    private string Description(string argument, bool isRequired) => isRequired ? argument : $"{argument}?";

    /// <summary>
    /// Defines the types of command line options supported by the help system.
    /// </summary>
    private enum OptionType
    {
        /// <summary>
        /// A single-value option that requires a value.
        /// </summary>
        Normal,

        /// <summary>
        /// A multi-value option that can accept multiple values.
        /// </summary>
        Multi,

        /// <summary>
        /// A flag option that doesn't require a value (boolean).
        /// </summary>
        Flag,
    }
}
