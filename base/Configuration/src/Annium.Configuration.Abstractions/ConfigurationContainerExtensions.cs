using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Configuration.Abstractions.Internal;

namespace Annium.Configuration.Abstractions;

/// <summary>
/// Extension methods for <see cref="IConfigurationContainer"/> covering deferred-source registration
/// and the terminal <see cref="BuildAsync"/> step that loads + merges all registered sources.
/// </summary>
public static class ConfigurationContainerExtensions
{
    /// <summary>
    /// Empty result reused when an optional source fails to load.
    /// </summary>
    private static readonly IReadOnlyDictionary<string[], string> _empty = new Dictionary<string[], string>();

    /// <summary>
    /// Registers a deferred object-configuration source. The object is flattened into key/value
    /// pairs at <see cref="BuildAsync"/> time.
    /// </summary>
    /// <typeparam name="TContainer">Container type</typeparam>
    /// <param name="container">The configuration container</param>
    /// <param name="config">The configuration object to add (may be null)</param>
    /// <param name="optional">When true, a flatten failure is swallowed (empty contribution).</param>
    /// <returns>The container for method chaining</returns>
    public static TContainer Add<TContainer>(this TContainer container, object? config, bool optional = false)
        where TContainer : IConfigurationContainer
    {
        container.AddSource(new ObjectConfigurationSource(config, optional));
        return container;
    }

    /// <summary>
    /// Loads every registered source in parallel, then merges results in registration order
    /// (later sources override earlier ones). Sources marked <c>Optional</c> swallow load
    /// failures and contribute an empty dictionary; any non-optional failures surface as a
    /// single <see cref="AggregateException"/>.
    /// </summary>
    /// <param name="container">Container whose sources to flush</param>
    /// <param name="ct">Cancellation token forwarded to each source</param>
    /// <returns>Task that completes once all loads have merged into the container</returns>
    public static async Task BuildAsync(this IConfigurationContainer container, CancellationToken ct = default)
    {
        var sources = container.Sources;
        if (sources.Count == 0)
            return;

        var loads = sources
            .Select(async s =>
            {
                try
                {
                    var data = await s.LoadAsync(ct);
                    return (source: s, data, error: null);
                }
                catch (Exception ex)
                {
                    return (source: s, data: _empty, error: (Exception?)ex);
                }
            })
            .ToArray();

        var results = await Task.WhenAll(loads);

        var failures = results.Where(r => r.error is not null && !r.source.Optional).Select(r => r.error!).ToArray();

        if (failures.Length > 0)
            throw new AggregateException("Configuration source(s) failed to load", failures);

        foreach (var (_, data, _) in results)
            container.Add(data);
    }
}
