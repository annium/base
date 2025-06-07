using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Annium.Configuration.Yaml.Internal;

// ReSharper disable once CheckNamespace
namespace Annium.Configuration.Abstractions;

/// <summary>
/// Extension methods for IConfigurationContainer to add YAML configuration
/// </summary>
public static class ConfigurationContainerExtensions
{
    /// <summary>
    /// Adds YAML configuration from a file to the container
    /// </summary>
    /// <param name="container">The configuration container</param>
    /// <param name="path">Path to the YAML file</param>
    /// <param name="optional">Whether the file is optional</param>
    /// <returns>The container for method chaining</returns>
    public static TContainer AddYamlFile<TContainer>(this TContainer container, string path, bool optional = false)
        where TContainer : IConfigurationContainer
    {
        path = Path.GetFullPath(path);
        if (!File.Exists(path))
            if (optional)
                return container;
            else
                throw new FileNotFoundException($"Yaml configuration file {path} not found and is not optional");

        var raw = File.ReadAllText(path);
        var configuration = new YamlConfigurationProvider(raw).Read();

        container.Add(configuration);

        return container;
    }

    /// <summary>
    /// Adds YAML configuration from a remote URI to the container asynchronously
    /// </summary>
    /// <param name="container">The configuration container</param>
    /// <param name="uri">URI to fetch YAML configuration from</param>
    /// <param name="optional">Whether the remote configuration is optional</param>
    /// <returns>Task containing the container for method chaining</returns>
    public static async Task<TContainer> AddRemoteYamlAsync<TContainer>(
        this TContainer container,
        string uri,
        bool optional = false
    )
        where TContainer : IConfigurationContainer
    {
        var client = new HttpClient();
        var message = new HttpRequestMessage(HttpMethod.Get, uri);
        var response = await client.SendAsync(message);
        if (!response.IsSuccessStatusCode)
            if (optional)
                return container;
            else
                throw new FileNotFoundException($"Yaml configuration not available at {uri} and is not optional");

        var raw = await response.Content.ReadAsStringAsync();
        var configuration = new YamlConfigurationProvider(raw).Read();

        container.Add(configuration);

        return container;
    }
}
