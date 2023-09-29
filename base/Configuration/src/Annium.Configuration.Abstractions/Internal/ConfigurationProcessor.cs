using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Annium.Core.Mapper;
using Annium.Core.Runtime.Types;
using Annium.Reflection;

namespace Annium.Configuration.Abstractions.Internal;

internal class ConfigurationProcessor<T>
    where T : new()
{
    private readonly ITypeManager _typeManager;
    private readonly IMapper _mapper;
    private readonly IReadOnlyDictionary<string[], string> _config;
    private readonly Stack<string> _context = new();
    private string[] Path => _context.Reverse().ToArray();

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

    public T Process()
    {
        return (T)(Process(typeof(T)) ?? default!);
    }

    private object? Process(Type type)
    {
        if (type.IsEnum || type.IsNullableValueType() || _mapper.HasMap(string.Empty, type))
            return ProcessValue(type);
        if (type.IsGenericType &&
            type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            return ProcessDictionary(type);
        if (type.IsGenericType &&
            type.GetGenericTypeDefinition() == typeof(List<>))
            return ProcessList(type);
        if (type.IsArray)
            return ProcessArray(type);
        return ProcessObject(type);
    }

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
            var key = _mapper.Map(name, keyType) ?? throw new InvalidOperationException($"Key at {keyType} is mapped to null");
            result[key] = Process(valueType);
            _context.Pop();
        }

        return result;
    }

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

    private object ProcessArray(Type type)
    {
        var elementType = type.GetElementType()!;
        var raw = (IList)ProcessList(typeof(List<>).MakeGenericType(elementType));

        var result = (IList)Array.CreateInstance(elementType, raw.Count);

        for (var index = 0; index < raw.Count; index++)
            result[index] = raw[index];

        return result;
    }

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
            type = _typeManager.ResolveByKey(key!, type)
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

    private object? ProcessValue(Type type)
    {
        if (!_config.TryGetValue(Path, out var value))
            throw new ArgumentException($"Key {string.Join('.', Path)} not found in configuration.");

        return _mapper.Map(value, Nullable.GetUnderlyingType(type) ?? type);
    }

    private string[] GetDescendants()
    {
        var path = Normalize(Path);
        if (path.Length == 0)
            return _config.Keys.Select(k => k.First()).Distinct().ToArray();

        return _config.Keys
            .Where(k => k.Length > path.Length)
            .Where(k => Normalize(k.Take(path.Length)).SequenceEqual(path))
            .Select(k => k.Skip(path.Length).First())
            .Distinct()
            .ToArray();
    }

    private bool KeyExists()
    {
        var path = Normalize(Path);
        if (path.Length == 0)
            return _config.Keys.Any();

        return _config.Keys
            .Where(k => k.Length >= path.Length)
            .Select(k => k.Take(path.Length))
            .Any(k => Normalize(k).SequenceEqual(path));
    }

    private string[] Normalize(IEnumerable<string> seq) => seq.Select(e => e.CamelCase()).ToArray();
}