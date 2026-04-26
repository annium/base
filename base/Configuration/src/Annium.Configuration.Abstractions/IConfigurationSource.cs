using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Configuration.Abstractions;

/// <summary>
/// Deferred configuration source — registered synchronously into a container, loaded once
/// during <see cref="ConfigurationContainerExtensions.BuildAsync"/>. The container snapshots
/// registered sources, loads them in parallel, and merges results in registration order.
/// </summary>
public interface IConfigurationSource
{
    /// <summary>
    /// When true, a load failure (missing file, network error, timeout) is silently swallowed
    /// and the source contributes an empty dictionary. When false, the failure surfaces from
    /// <see cref="ConfigurationContainerExtensions.BuildAsync"/> as part of an aggregate exception.
    /// </summary>
    bool Optional { get; }

    /// <summary>
    /// Loads the source's flat configuration data. Keys are path segments (e.g.,
    /// <c>["section", "subsection", "key"]</c>); values are stringified leaves.
    /// </summary>
    /// <param name="ct">Cancellation token forwarded from the caller of <c>BuildAsync</c>.</param>
    /// <returns>The loaded configuration dictionary.</returns>
    ValueTask<IReadOnlyDictionary<string[], string>> LoadAsync(CancellationToken ct);
}
