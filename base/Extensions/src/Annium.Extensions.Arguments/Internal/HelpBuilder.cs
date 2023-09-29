using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Annium.Extensions.Arguments.Internal;

internal class HelpBuilder : IHelpBuilder
{
    private readonly IConfigurationProcessor _configurationProcessor;

    public HelpBuilder(
        IConfigurationProcessor configurationProcessor
    )
    {
        _configurationProcessor = configurationProcessor;
    }

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
                o => Description(Option(o.Key) + (o.Value.Item3 == OptionType.Multi ? "[]" : string.Empty), o.Value.Item2)
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

    private IReadOnlyDictionary<string, bool> GetPositions(Type[] types) => types
        .SelectMany(t => _configurationProcessor.GetPropertiesWithAttribute<PositionAttribute>(t))
        .OrderBy(e => e.attribute.Position)
        .ToDictionary(
            e => e.property.Name.KebabCase(),
            e => e.attribute.IsRequired
        );

    private IReadOnlyDictionary<string, ValueTuple<string?, bool, OptionType>> GetOptions(Type[] types) => types
        .SelectMany(t => _configurationProcessor.GetPropertiesWithAttribute<OptionAttribute>(t))
        .OrderBy(e => e.attribute.IsRequired)
        .ThenBy(e => e.property.Name)
        .ToDictionary(
            e => e.property.Name.KebabCase(),
            e => (
                e.attribute.Alias?.KebabCase(),
                e.attribute.IsRequired,
                e.property.PropertyType.IsArray
                    ? OptionType.Multi
                    : e.property.PropertyType == typeof(bool)
                        ? OptionType.Flag
                        : OptionType.Normal
            )
        );

    private IReadOnlyDictionary<string, string> GetHelps(Type[] types) => types
        .SelectMany(t => _configurationProcessor.GetPropertiesWithAttribute<HelpAttribute>(t))
        .ToDictionary(
            e => e.property.Name.KebabCase(),
            e => e.attribute.Help
        );

    private string? GetRaw(Type[] types) => types
        .SelectMany(t => _configurationProcessor.GetPropertiesWithAttribute<RawAttribute>(t))
        .Select(e => e.property.Name.KebabCase())
        .FirstOrDefault();

    private string Option(string argument) => $"{Constants.OptionSign}{argument}";

    private string Usage(string argument, bool isRequired) => isRequired ? $"<{argument}>" : $"[{argument}]";

    private string Description(string argument, bool isRequired) => isRequired ? argument : $"{argument}?";

    private enum OptionType
    {
        Normal,
        Multi,
        Flag,
    }
}