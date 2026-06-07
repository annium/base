using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using Annium.Localization.Abstractions;
using Annium.Reflection;
using YamlDotNet.Core;
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
    /// Cache of loaded locales indexed by file path. Each value is wrapped in a <see cref="Lazy{T}"/>
    /// so the cache stays lock-free while each file is read and parsed exactly once.
    /// </summary>
    private readonly ConcurrentDictionary<string, Lazy<IReadOnlyDictionary<string, string>>> _locales = new();

    /// <summary>
    /// Shared empty locale returned when a file is missing, empty, or malformed.
    /// </summary>
    private static readonly IReadOnlyDictionary<string, string> _empty = new Dictionary<string, string>();

    /// <summary>
    /// Loads a locale dictionary for the specified type and culture from YAML files.
    /// </summary>
    /// <param name="target">The target type to load locale for</param>
    /// <param name="culture">The culture to load locale for</param>
    /// <returns>A dictionary containing the locale entries for the specified culture</returns>
    public IReadOnlyDictionary<string, string> LoadLocale(Type target, CultureInfo culture)
    {
        // AppContext.BaseDirectory is the trim/single-file-safe replacement for Assembly.Location
        // (which returns empty under those publish modes); resolves to the process's runtime
        // directory — the same place locale YAMLs ship into.
        var assembly = target.GetTypeInfo().Assembly;
        var location = AppContext.BaseDirectory;

        var assemblyNamePath = Path.Combine(assembly.ShortName().Split('.'));
        var targetNamespacePath = Path.Combine(target.Namespace?.Split('.') ?? Array.Empty<string>());

        // Root both paths under a common synthetic absolute base so GetRelativePath operates on
        // absolute paths (its documented contract), yielding consistent results across platforms.
        // GetRelativePath also throws ArgumentException on an empty path, which happens for types
        // in the global namespace (target.Namespace is null); fall back to the assembly root.
        var root = Path.DirectorySeparatorChar.ToString();
        var localeRelativePath =
            targetNamespacePath.Length == 0
                ? string.Empty
                : Path.GetRelativePath(Path.Combine(root, assemblyNamePath), Path.Combine(root, targetNamespacePath));

        // a namespace not nested under the assembly short name yields a "../"-escaping relative
        // path; fall back to the assembly root rather than resolving a locale outside the deploy directory.
        if (localeRelativePath == ".." || localeRelativePath.StartsWith(".." + Path.DirectorySeparatorChar))
            localeRelativePath = string.Empty;

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
    private IReadOnlyDictionary<string, string> ResolveLocale(string file) =>
        _locales.GetOrAdd(file, f => new Lazy<IReadOnlyDictionary<string, string>>(() => LoadFile(f))).Value;

    /// <summary>
    /// Reads and parses a YAML locale file, returning an empty dictionary if it does not exist or is empty.
    /// </summary>
    /// <param name="file">The path to the YAML locale file</param>
    /// <returns>A dictionary containing the locale entries from the file</returns>
    private IReadOnlyDictionary<string, string> LoadFile(string file)
    {
        if (!File.Exists(file))
            return _empty;

        try
        {
            return _deserializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(file)) ?? _empty;
        }
        catch (Exception e) when (e is YamlException or IOException or UnauthorizedAccessException)
        {
            // a malformed, unreadable, or vanished (Exists→read race) locale file degrades to no
            // translations — the same graceful outcome as a missing file — so a bad file leaves entries
            // untranslated instead of crashing the localizer (and permanently faulting the cached Lazy)
            return _empty;
        }
    }
}
