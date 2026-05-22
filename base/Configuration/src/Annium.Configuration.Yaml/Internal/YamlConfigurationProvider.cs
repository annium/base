using System;
using System.Collections.Generic;
using System.IO;
using Annium.Configuration.Abstractions;
using YamlDotNet.RepresentationModel;

namespace Annium.Configuration.Yaml.Internal;

/// <summary>
/// Configuration provider that reads configuration data from YAML
/// </summary>
internal class YamlConfigurationProvider : ConfigurationProviderBase
{
    /// <summary>
    /// Raw YAML string to process
    /// </summary>
    private readonly string _raw;

    /// <summary>
    /// Initializes a new instance of YamlConfigurationProvider
    /// </summary>
    /// <param name="raw">Raw YAML string to process</param>
    public YamlConfigurationProvider(string raw)
    {
        _raw = raw;
    }

    /// <summary>
    /// Reads configuration data from YAML and returns it as a dictionary
    /// </summary>
    /// <returns>Dictionary containing configuration keys and values</returns>
    public override IReadOnlyDictionary<string[], string> Read()
    {
        Init();

        var stream = new YamlStream();
        using var reader = new StringReader(_raw);
        stream.Load(reader);

        if (stream.Documents.Count == 0)
            return Data;

        if (stream.Documents[0].RootNode is not YamlMappingNode root)
            throw new InvalidOperationException(
                $"YAML root must be a mapping node, got {stream.Documents[0].RootNode.GetType().Name}"
            );

        Process(root);

        return Data;
    }

    /// <summary>
    /// Processes a YAML mapping node by iterating through its key-value pairs
    /// </summary>
    /// <param name="node">YAML mapping node to process</param>
    private void Process(YamlMappingNode node)
    {
        foreach (var (key, value) in node.Children)
        {
            if (key is not YamlScalarNode scalarKey)
                throw new InvalidOperationException(
                    $"YAML mapping key must be a scalar, got {key.GetType().Name}"
                );

            Context.Push(scalarKey.Value!);

            if (value is YamlMappingNode map)
                Process(map);
            else if (value is YamlSequenceNode seq)
                Process(seq);
            else if (value is YamlScalarNode scalarValue)
                Process(scalarValue);
            else
                throw new InvalidOperationException(
                    $"Unexpected YAML node type {value.GetType().Name} at {string.Join(".", Path)}"
                );

            Context.Pop();
        }
    }

    /// <summary>
    /// Processes a YAML sequence node by iterating through its items with indices
    /// </summary>
    /// <param name="node">YAML sequence node to process</param>
    private void Process(YamlSequenceNode node)
    {
        var index = 0;
        foreach (var item in node)
        {
            Context.Push(index.ToString());

            if (item is YamlMappingNode map)
                Process(map);
            else if (item is YamlSequenceNode seq)
                Process(seq);
            else if (item is YamlScalarNode scalarItem)
                Process(scalarItem);
            else
                throw new InvalidOperationException(
                    $"Unexpected YAML node type {item.GetType().Name} at {string.Join(".", Path)}"
                );

            Context.Pop();
            index++;
        }
    }

    /// <summary>
    /// Processes a YAML scalar node by adding its value to the configuration data
    /// </summary>
    /// <param name="token">YAML scalar node to process</param>
    private void Process(YamlScalarNode token)
    {
        Data[Path] = token.Value!;
    }
}
