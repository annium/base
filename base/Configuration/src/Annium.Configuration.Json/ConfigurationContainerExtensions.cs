using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Annium.Configuration.Abstractions;
using Annium.Configuration.Json.Internal;

namespace Annium.Configuration.Json;

/// <summary>
/// Extension methods for IConfigurationContainer to add JSON configuration
/// </summary>
public static class ConfigurationContainerExtensions
{
    /// <summary>
    /// Adds JSON configuration from a file to the container
    /// </summary>
    /// <param name="container">The configuration container</param>
    /// <param name="path">Path to the JSON file</param>
    /// <param name="optional">Whether the file is optional</param>
    /// <returns>The container for method chaining</returns>
    public static TContainer AddJsonFile<TContainer>(this TContainer container, string path, bool optional = false)
        where TContainer : IConfigurationContainer
    {
        path = Path.GetFullPath(path);
        if (!File.Exists(path))
            if (optional)
                return container;
            else
                throw new FileNotFoundException($"Json configuration file {path} not found and is not optional");

        var raw = File.ReadAllText(path);
        var configuration = new JsonConfigurationProvider(raw).Read();

        container.Add(configuration);

        return container;
    }

    /// <summary>
    /// Adds JSON configuration from a remote URI to the container asynchronously
    /// </summary>
    /// <param name="container">The configuration container</param>
    /// <param name="uri">URI to fetch JSON configuration from</param>
    /// <param name="optional">Whether the remote configuration is optional</param>
    /// <returns>Task containing the container for method chaining</returns>
    public static async Task<TContainer> AddRemoteJsonAsync<TContainer>(
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
                throw new FileNotFoundException($"Json configuration not available at {uri} and is not optional");

        var raw = await response.Content.ReadAsStringAsync();
        var configuration = new JsonConfigurationProvider(raw).Read();

        container.Add(configuration);

        return container;
    }
}
