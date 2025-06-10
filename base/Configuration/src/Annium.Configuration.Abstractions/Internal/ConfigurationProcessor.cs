using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Annium.Core.Mapper;
using Annium.Core.Runtime.Types;
using Annium.Reflection.Types;

namespace Annium.Configuration.Abstractions.Internal;

/// <summary>
/// Processes configuration data and converts it to strongly typed objects
/// </summary>
internal class ConfigurationProcessor<T>
    where T : new()
{
    /// <summary>
    /// Type manager for resolving types during processing
    /// </summary>
    private readonly ITypeManager _typeManager;

    /// <summary>
    /// Mapper for converting between different data types
    /// </summary>
    private readonly IMapper _mapper;

    /// <summary>
    /// Configuration data to process
    /// </summary>
    private readonly IReadOnlyDictionary<string[], string> _config;

    /// <summary>
    /// Stack maintaining the current path context during processing
    /// </summary>
    private readonly Stack<string> _context = new();

    /// <summary>
    /// Gets the current path as an array of strings from the context stack
    /// </summary>
    private string[] Path => _context.Reverse().ToArray();

    /// <summary>
    /// Initializes a new instance of ConfigurationProcessor
    /// </summary>
    /// <param name="typeManager">Type manager for type resolution</param>
    /// <param name="mapper">Mapper for data conversion</param>
    /// <param name="config">Configuration data to process</param>
    public ConfigurationProcessor(
        ITypeManager typeManager,
        IMapper mapper,
        IReadOnlyDictionary<string[], string> config
    )
    {
        _typeManager = typeManager;
        _mapper = mapper;
        _config = config;
    }

    /// <summary>
    /// Processes the configuration data and returns an instance of type T
    /// </summary>
    /// <returns>Configured instance of type T</returns>
    public T Process()
    {
        return (T)(Process(typeof(T)) ?? default!);
    }

    /// <summary>
    /// Processes configuration data for a specific type
    /// </summary>
    /// <param name="type">Type to process configuration for</param>
    /// <returns>Configured instance of the specified type</returns>
    private object? Process(Type type)
    {
        if (type.IsEnum || type.IsNullableValueType() || _mapper.HasMap(string.Empty, type))
            return ProcessValue(type);
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            return ProcessDictionary(type);
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            return ProcessList(type);
        if (type.IsArray)
            return ProcessArray(type);
        return ProcessObject(type);
    }

    /// <summary>
    /// Processes configuration data for dictionary types
    /// </summary>
    /// <param name="type">Dictionary type to process</param>
    /// <returns>Configured dictionary instance</returns>
    private object ProcessDictionary(Type type)
    {
        var keyType = type.GetGenericArguments()[0];
        var valueType = type.GetGenericArguments()[1];

        // var items = config.Where(e => e.Key.StartsWith(path, StringComparison.OrdinalIgnoreCase) && e.Key.Length > path.Length).ToArray();
        var items = GetDescendants();
        var result = (IDictionary)Activator.CreateInstance(type)!;

        foreach (var name in items)
        {
            // var name = key.Substring(path.Length + separator.Length).Split(separator) [0];
            _context.Push(name);
            var key =
                _mapper.Map(name, keyType)
                ?? throw new InvalidOperationException($"Key at {keyType} is mapped to null");
            result[key] = Process(valueType);
            _context.Pop();
        }

        return result;
    }

    /// <summary>
    /// Processes configuration data for list types
    /// </summary>
    /// <param name="type">List type to process</param>
    /// <returns>Configured list instance</returns>
    private object ProcessList(Type type)
    {
        var elementType = type.GetGenericArguments()[0];
        var result = (IList)Activator.CreateInstance(type)!;

        var items = GetDescendants();

        foreach (var index in items)
        {
            _context.Push(index);
            result.Add(Process(elementType));
            _context.Pop();
        }

        return result;
    }

    /// <summary>
    /// Processes configuration data for array types
    /// </summary>
    /// <param name="type">Array type to process</param>
    /// <returns>Configured array instance</returns>
    private object ProcessArray(Type type)
    {
        var elementType = type.GetElementType()!;
        var raw = (IList)ProcessList(typeof(List<>).MakeGenericType(elementType));

        var result = (IList)Array.CreateInstance(elementType, raw.Count);

        for (var index = 0; index < raw.Count; index++)
            result[index] = raw[index];

        return result;
    }

    /// <summary>
    /// Processes configuration data for object types
    /// </summary>
    /// <param name="type">Object type to process</param>
    /// <returns>Configured object instance</returns>
    private object ProcessObject(Type type)
    {
        if (type.IsAbstract || type.IsInterface)
        {
            var resolutionKeyProperty = _typeManager.GetResolutionKeyProperty(type);
            if (resolutionKeyProperty is null)
                throw new ArgumentException($"Can't resolve abstract type {type}");

            _context.Push(resolutionKeyProperty.Name);
            var hasKey = _config.TryGetValue(Path, out var rawKey);
            _context.Pop();
            if (!hasKey || rawKey is null)
                return null!;

            var key = _mapper.Map(rawKey, resolutionKeyProperty.PropertyType);
            type =
                _typeManager.ResolveByKey(key!, type)
                ?? throw new ArgumentException($"Can't resolve abstract type {type} with key {key}");
        }

        var result = Activator.CreateInstance(type)!;
        var properties = type.GetProperties().Where(e => e.CanWrite).ToArray();
        foreach (var property in properties)
        {
            _context.Push(property.Name);
            if (KeyExists())
                property.SetValue(result, Process(property.PropertyType));
            _context.Pop();
        }

        return result;
    }

    /// <summary>
    /// Processes configuration data for value types
    /// </summary>
    /// <param name="type">Value type to process</param>
    /// <returns>Configured value instance</returns>
    private object? ProcessValue(Type type)
    {
        if (!_config.TryGetValue(Path, out var value))
            throw new ArgumentException($"Key {string.Join('.', Path)} not found in configuration.");

        return _mapper.Map(value, Nullable.GetUnderlyingType(type) ?? type);
    }

    /// <summary>
    /// Gets descendant keys from the current path context
    /// </summary>
    /// <returns>Array of descendant key strings</returns>
    private string[] GetDescendants()
    {
        var path = Normalize(Path);
        if (path.Length == 0)
            return _config.Keys.Select(k => k.First()).Distinct().ToArray();

        return _config
            .Keys.Where(k => k.Length > path.Length)
            .Where(k => Normalize(k.Take(path.Length)).SequenceEqual(path))
            .Select(k => k.Skip(path.Length).First())
            .Distinct()
            .ToArray();
    }

    /// <summary>
    /// Checks if a key exists at the current path context
    /// </summary>
    /// <returns>True if key exists, false otherwise</returns>
    private bool KeyExists()
    {
        var path = Normalize(Path);
        if (path.Length == 0)
            return _config.Keys.Any();

        return _config
            .Keys.Where(k => k.Length >= path.Length)
            .Select(k => k.Take(path.Length))
            .Any(k => Normalize(k).SequenceEqual(path));
    }

    /// <summary>
    /// Normalizes a sequence of strings to camel case
    /// </summary>
    /// <param name="seq">Sequence of strings to normalize</param>
    /// <returns>Array of normalized strings</returns>
    private string[] Normalize(IEnumerable<string> seq) => seq.Select(e => e.CamelCase()).ToArray();
}
