using System.Collections.Generic;
using System.Text.Json;
using Annium.Configuration.Abstractions;

namespace Annium.Configuration.Json.Internal;

/// <summary>
/// Configuration provider that reads configuration data from JSON
/// </summary>
internal class JsonConfigurationProvider : ConfigurationProviderBase
{
    /// <summary>
    /// Raw JSON string to process
    /// </summary>
    private readonly string _raw;

    /// <summary>
    /// Initializes a new instance of JsonConfigurationProvider
    /// </summary>
    /// <param name="raw">Raw JSON string to process</param>
    public JsonConfigurationProvider(string raw)
    {
        _raw = raw;
    }

    /// <summary>
    /// Reads configuration data from JSON and returns it as a dictionary
    /// </summary>
    /// <returns>Dictionary containing configuration keys and values</returns>
    public override IReadOnlyDictionary<string[], string> Read()
    {
        Init();

        var element = JsonDocument.Parse(_raw).RootElement;

        Process(element);

        return Data;
    }

    /// <summary>
    /// Processes a JSON object by iterating through its properties
    /// </summary>
    /// <param name="token">JSON element representing an object</param>
    private void ProcessObject(JsonElement token)
    {
        foreach (var property in token.EnumerateObject())
        {
            Context.Push(property.Name);
            Process(property.Value);
            Context.Pop();
        }
    }

    /// <summary>
    /// Processes a JSON array by iterating through its items with indices
    /// </summary>
    /// <param name="token">JSON element representing an array</param>
    private void ProcessArray(JsonElement token)
    {
        var index = 0;
        foreach (var item in token.EnumerateArray())
        {
            Context.Push(index.ToString());
            Process(item);
            Context.Pop();
            index++;
        }
    }

    /// <summary>
    /// Processes a JSON leaf value by adding it to the configuration data
    /// </summary>
    /// <param name="token">JSON element representing a leaf value</param>
    private void ProcessLeaf(JsonElement token)
    {
        Data[Path] = token.ToString();
    }

    /// <summary>
    /// Processes a JSON element based on its type
    /// </summary>
    /// <param name="element">JSON element to process</param>
    private void Process(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                ProcessObject(element);
                break;
            case JsonValueKind.Array:
                ProcessArray(element);
                break;
            default:
                ProcessLeaf(element);
                break;
        }
    }
}
