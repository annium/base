using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Annium.Core.Mapper;

namespace Annium.Extensions.Arguments.Internal;

/// <summary>
/// Builds typed configuration objects from command line arguments by processing raw arguments
/// and mapping them to properties decorated with position, option, and raw attributes.
/// </summary>
internal class ConfigurationBuilder : IConfigurationBuilder
{
    /// <summary>
    /// Processes raw command line arguments into structured data.
    /// </summary>
    private readonly IArgumentProcessor _argumentProcessor;

    /// <summary>
    /// Processes configuration types to extract property metadata.
    /// </summary>
    private readonly IConfigurationProcessor _configurationProcessor;

    /// <summary>
    /// Maps string values to appropriate target types.
    /// </summary>
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the ConfigurationBuilder class with required dependencies.
    /// </summary>
    /// <param name="argumentProcessor">The processor for parsing raw command line arguments</param>
    /// <param name="configurationProcessor">The processor for extracting property metadata from configuration types</param>
    /// <param name="mapper">The mapper for converting string values to target types</param>
    public ConfigurationBuilder(
        IArgumentProcessor argumentProcessor,
        IConfigurationProcessor configurationProcessor,
        IMapper mapper
    )
    {
        _argumentProcessor = argumentProcessor;
        _configurationProcessor = configurationProcessor;
        _mapper = mapper;
    }

    /// <summary>
    /// Builds a typed configuration object from command line arguments by parsing and mapping
    /// positional arguments, flags, options, and raw content to appropriate properties.
    /// </summary>
    /// <typeparam name="T">The configuration type to build, must have a parameterless constructor</typeparam>
    /// <param name="args">Array of command line arguments to process</param>
    /// <returns>A fully populated configuration object of type T</returns>
    public T Build<T>(string[] args)
        where T : new()
    {
        var options = _configurationProcessor.GetPropertiesWithAttribute<OptionAttribute>(typeof(T));

        EnsureNamesAreUnique(options);

        var raw = _argumentProcessor.Compose(args, Spec(options));

        var value = new T();

        SetPositions(value, raw.Positions.ToArray());
        SetFlags(value, raw.Flags.ToArray());
        SetOptions(value, raw.Options, raw.MultiOptions);
        SetRaw(value, raw.Raw);

        return value;
    }

    /// <summary>
    /// Sets positional argument values on the configuration object based on position attributes.
    /// </summary>
    /// <typeparam name="T">The configuration type</typeparam>
    /// <param name="value">The configuration object instance to populate</param>
    /// <param name="positions">Array of positional argument values from command line</param>
    private void SetPositions<T>(T value, string[] positions)
    {
        var properties = _configurationProcessor
            .GetPropertiesWithAttribute<PositionAttribute>(typeof(T))
            .OrderBy(e => e.attribute.Position);

        var i = 1;
        foreach (var (property, attribute) in properties)
        {
            if (attribute.Position != i)
                throw new ArgumentParseException(
                    $"Position argument expected to have position '{i}', but got position '{attribute.Position}'"
                );

            if (i > positions.Length)
                if (attribute.IsRequired)
                    throw new ArgumentParseException($"Required position argument '{property.Name}' has no value");
                else
                    break;

            property.SetValue(value, GetValue(property, property.PropertyType, positions[i - 1]));
            i++;
        }
    }

    /// <summary>
    /// Sets flag values on boolean properties of the configuration object.
    /// </summary>
    /// <typeparam name="T">The configuration type</typeparam>
    /// <param name="value">The configuration object instance to populate</param>
    /// <param name="flags">Array of flag names that were present in command line</param>
    private void SetFlags<T>(T value, string[] flags)
    {
        var properties = _configurationProcessor
            .GetPropertiesWithAttribute<OptionAttribute>(typeof(T))
            .Where(e => e.property.PropertyType == typeof(bool))
            .ToArray();

        foreach (var (property, attribute) in properties)
        {
            var key = FindOptionName(flags, property.Name, attribute.Alias);
            property.SetValue(value, key != null);
        }
    }

    /// <summary>
    /// Sets option values on properties of the configuration object, handling both single and multi-value options.
    /// </summary>
    /// <typeparam name="T">The configuration type</typeparam>
    /// <param name="value">The configuration object instance to populate</param>
    /// <param name="plainOptions">Dictionary of single-value options from command line</param>
    /// <param name="multiOptions">Dictionary of multi-value options from command line</param>
    private void SetOptions<T>(
        T value,
        IReadOnlyDictionary<string, string> plainOptions,
        IReadOnlyDictionary<string, IReadOnlyCollection<string>> multiOptions
    )
    {
        var properties = _configurationProcessor.GetPropertiesWithAttribute<OptionAttribute>(typeof(T));
        var plainProperties = properties
            .Where(e => e.property.PropertyType != typeof(bool) && !e.property.PropertyType.IsArray)
            .ToArray();

        var plainOptionsKeys = plainOptions.Keys.ToArray();
        var multiOptionsKeys = multiOptions.Keys.ToArray();

        // for base properties - set values from options
        foreach (var (property, attribute) in plainProperties)
        {
            var key = FindOptionName(plainOptionsKeys, property.Name, attribute.Alias);
            if (key == null)
                if (attribute.IsRequired)
                    throw new ArgumentParseException($"Required option argument '{property.Name}' has no value");
                else
                    continue;

            property.SetValue(value, GetValue(property, property.PropertyType, plainOptions[key]));
        }

        var arrayProperties = _configurationProcessor
            .GetPropertiesWithAttribute<OptionAttribute>(typeof(T))
            .Where(e => e.property.PropertyType.IsArray)
            .ToArray();

        // for array properties - set values from multi-options with fallback to options
        foreach (var (property, attribute) in arrayProperties)
        {
            string? key;
            string[] raw;
            if ((key = FindOptionName(multiOptionsKeys, property.Name, attribute.Alias)) != null)
                raw = multiOptions[key].ToArray();
            else if ((key = FindOptionName(plainOptionsKeys, property.Name, attribute.Alias)) != null)
                raw = new[] { plainOptions[key] };
            else if (attribute.IsRequired)
                throw new ArgumentParseException($"Required multi option argument '{property.Name}' has no value");
            else
                continue;

            var type = property.PropertyType.GetElementType()!;
            var array = (IList)Array.CreateInstance(type, raw.Length);
            for (var i = 0; i < array.Count; i++)
                array[i] = GetValue(property, type, raw[i]);

            property.SetValue(value, array);
        }
    }

    /// <summary>
    /// Sets the raw argument value on the property decorated with RawAttribute.
    /// </summary>
    /// <typeparam name="T">The configuration type</typeparam>
    /// <param name="value">The configuration object instance to populate</param>
    /// <param name="raw">The raw argument string from command line</param>
    private void SetRaw<T>(T value, string raw)
    {
        var (property, _) = _configurationProcessor
            .GetPropertiesWithAttribute<RawAttribute>(typeof(T))
            .FirstOrDefault();

        if (property != null!)
            property.SetValue(value, GetValue(property, property.PropertyType, raw));
    }

    /// <summary>
    /// Finds the matching option name from available names, checking both the property name and alias.
    /// </summary>
    /// <param name="names">Collection of available option names</param>
    /// <param name="name">The primary property name to search for</param>
    /// <param name="alias">The optional alias name to search for</param>
    /// <returns>The matching option name if found, null otherwise</returns>
    private string? FindOptionName(IReadOnlyCollection<string> names, string name, string? alias)
    {
        // the names to match against come from the lexer, which normalised them; so must these
        name = name.PascalCase();

        if (alias == null)
            return names.Contains(name) ? name : null;

        alias = alias.PascalCase();

        if (names.Contains(alias))
            return alias;

        if (names.Contains(name))
            return name;

        return null;
    }

    /// <summary>
    /// Describes the type's options for the lexer: every spelling mapped to the option it names, and which
    /// of those options are flags or collections. The spellings are normalised the same way the lexer
    /// normalises the token it reads - a property whose own name does not survive that unchanged, an
    /// acronym like URL, would otherwise never match.
    /// </summary>
    /// <param name="options">The option-bearing properties of the configuration type.</param>
    /// <returns>The spec describing them.</returns>
    private static OptionSpec Spec(IReadOnlyCollection<(PropertyInfo property, OptionAttribute attribute)> options)
    {
        var names = new Dictionary<string, string>();
        var kinds = new Dictionary<string, OptionKind>();

        foreach (var (property, attribute) in options)
        {
            var canonical = property.Name.PascalCase();
            foreach (var spelling in Names(property.Name, attribute.Alias))
                names[spelling] = canonical;

            kinds[canonical] =
                property.PropertyType == typeof(bool) ? OptionKind.Flag
                : property.PropertyType.IsArray ? OptionKind.Many
                : OptionKind.Single;
        }

        return new OptionSpec(names, kinds);
    }

    /// <summary>
    /// The normalised names an option answers to: its property name, and its alias when it has one.
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <param name="alias">The alias, if any.</param>
    /// <returns>The names the option answers to.</returns>
    private static IEnumerable<string> Names(string name, string? alias) =>
        alias is null ? [name.PascalCase()] : [name.PascalCase(), alias.PascalCase()];

    /// <summary>
    /// Fails if two properties answer to the same name, which would otherwise route a value to whichever
    /// of them happened to be looked up first - and, when one of them is a flag, make the other's option
    /// lex as a flag for the whole type.
    /// </summary>
    /// <param name="options">The option-bearing properties of the configuration type.</param>
    private static void EnsureNamesAreUnique(
        IReadOnlyCollection<(PropertyInfo property, OptionAttribute attribute)> options
    )
    {
        var claims = new Dictionary<string, string>();
        foreach (var (property, attribute) in options)
        foreach (var claimed in Names(property.Name, attribute.Alias))
        {
            // a spelling starting with a digit cannot be told from a negative number on a command line;
            // saying so here beats reading it as a number on every invocation
            if (char.IsAsciiDigit(claimed[0]))
                throw new ArgumentParseException(
                    $"Option '{property.Name}' answers to '{claimed}', which reads as a number"
                );

            if (claims.TryGetValue(claimed, out var other) && other != property.Name)
                throw new ArgumentParseException($"Options '{other}' and '{property.Name}' both answer to '{claimed}'");

            claims[claimed] = property.Name;
        }
    }

    /// <summary>
    /// Converts a string value to the target type, validating against allowed values if specified.
    /// </summary>
    /// <param name="property">The property information for validation context</param>
    /// <param name="type">The target type to convert to</param>
    /// <param name="value">The string value to convert</param>
    /// <returns>The converted value of the target type</returns>
    private object? GetValue(PropertyInfo property, Type type, string value)
    {
        var values = property.GetCustomAttribute<ValuesAttribute>()?.Values;
        if (values != null && !values.Contains(value))
            throw new ArgumentParseException(
                $"Given value '{value}' isn't in allowed values: {string.Join(", ", values)}"
            );

        // every other way a command line can be wrong is reported as an ArgumentParseException; a value the
        // mapper cannot convert used to come out as whatever the conversion threw, so a caller catching
        // usage errors to print help missed exactly the most ordinary mistake of all
        try
        {
            return _mapper.Map(value, type);
        }
        catch (Exception e)
        {
            throw new ArgumentParseException(
                $"Given value '{value}' for '{property.Name}' is not a valid {type.FriendlyName()}",
                e
            );
        }
    }
}
