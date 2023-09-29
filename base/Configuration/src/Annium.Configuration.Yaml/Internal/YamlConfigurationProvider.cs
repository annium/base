using System.Collections.Generic;
using System.IO;
using Annium.Configuration.Abstractions;
using YamlDotNet.RepresentationModel;

namespace Annium.Configuration.Yaml.Internal;

internal class YamlConfigurationProvider : ConfigurationProviderBase
{
    private readonly string _raw;

    public YamlConfigurationProvider(string raw)
    {
        _raw = raw;
    }

    public override IReadOnlyDictionary<string[], string> Read()
    {
        Init();

        var stream = new YamlStream();
        using var reader = new StringReader(_raw);
        stream.Load(reader);

        if (stream.Documents.Count == 0)
            return Data;

        Process((YamlMappingNode)stream.Documents[0].RootNode);

        return Data;
    }

    private void Process(YamlMappingNode node)
    {
        foreach (var (key, value) in node.Children)
        {
            Context.Push(((YamlScalarNode)key).Value!);

            if (value is YamlMappingNode map)
                Process(map);
            else if (value is YamlSequenceNode seq)
                Process(seq);
            else
                Process((YamlScalarNode)value);

            Context.Pop();
        }
    }

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
            else
                Process((YamlScalarNode)item);

            Context.Pop();
            index++;
        }
    }

    private void Process(YamlScalarNode token)
    {
        Data[Path] = token.Value!;
    }
}