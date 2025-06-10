using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using Annium.Localization.Abstractions;
using Annium.Reflection;
using YamlDotNet.Serialization;

namespace Annium.Localization.Yaml;

/// <summary>
/// YAML-based locale storage implementation that loads localization files from the file system.
/// Provides caching and automatic path resolution based on assembly and namespace structure.
/// </summary>
internal class Storage : ILocaleStorage
{
    /// <summary>
    /// YAML deserializer instance for parsing locale files.
    /// </summary>
    private readonly IDeserializer _deserializer = new DeserializerBuilder().Build();

    /// <summary>
    /// Cache of loaded locales indexed by file path.
    /// </summary>
    private readonly IDictionary<string, IReadOnlyDictionary<string, string>> _locales =
        new Dictionary<string, IReadOnlyDictionary<string, string>>();

    /// <summary>
    /// Loads a locale dictionary for the specified type and culture from YAML files.
    /// </summary>
    /// <param name="target">The target type to load locale for</param>
    /// <param name="culture">The culture to load locale for</param>
    /// <returns>A dictionary containing the locale entries for the specified culture</returns>
    public IReadOnlyDictionary<string, string> LoadLocale(Type target, CultureInfo culture)
    {
        // TODO: upgrade to load locales from current directory (will require build task?)
        var assembly = target.GetTypeInfo().Assembly;
        var location = Path.GetDirectoryName(assembly.Location);

        var assemblyNamePath = Path.Combine(assembly.ShortName().Split('.'));
        var targetNamespacePath = Path.Combine(target.Namespace?.Split('.') ?? Array.Empty<string>());
        var localeRelativePath = Path.GetRelativePath(assemblyNamePath, targetNamespacePath);

        var file = Path.Combine(
            location ?? string.Empty,
            localeRelativePath,
            "locale",
            $"{culture.TwoLetterISOLanguageName}.yml"
        );

        return ResolveLocale(file);
    }

    /// <summary>
    /// Resolves and caches a locale dictionary from the specified YAML file.
    /// </summary>
    /// <param name="file">The path to the YAML locale file</param>
    /// <returns>A dictionary containing the locale entries from the file</returns>
    private IReadOnlyDictionary<string, string> ResolveLocale(string file)
    {
        lock (_locales)
        {
            if (_locales.TryGetValue(file, out var locale))
                return locale;

            locale = File.Exists(file)
                ? _deserializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(file))
                : new Dictionary<string, string>();

            _locales[file] = locale;

            return locale;
        }
    }
}
