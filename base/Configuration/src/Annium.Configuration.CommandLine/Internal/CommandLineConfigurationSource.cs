using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Configuration.Abstractions;

namespace Annium.Configuration.CommandLine.Internal;

/// <summary>
/// Deferred configuration source that parses command-line arguments. When constructed with
/// null args, reads <see cref="Environment.GetCommandLineArgs"/> at <see cref="LoadAsync"/>
/// time (skipping the executable path at index 0).
/// </summary>
internal sealed class CommandLineConfigurationSource : IConfigurationSource
{
    /// <summary>Override args, or null to read from the environment at load time.</summary>
    private readonly string[]? _args;

    /// <summary>Whether a parse failure is silenced.</summary>
    public bool Optional { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandLineConfigurationSource"/> class.
    /// </summary>
    /// <param name="args">The command-line arguments to parse, or <c>null</c> to read the process arguments.</param>
    /// <param name="optional">Whether a parse failure is silenced instead of thrown.</param>
    public CommandLineConfigurationSource(string[]? args, bool optional)
    {
        _args = args;
        Optional = optional;
    }

    /// <summary>
    /// Resolves args (override or environment) and parses them via the existing provider.
    /// Synchronous in nature — returned as a completed <see cref="ValueTask"/>.
    /// </summary>
    /// <param name="ct">Cancellation token (unused for synchronous parse).</param>
    /// <returns>Flattened command-line configuration.</returns>
    public ValueTask<IReadOnlyDictionary<string[], string>> LoadAsync(CancellationToken ct)
    {
        var resolved = _args ?? Environment.GetCommandLineArgs().Skip(1).ToArray();
        return new ValueTask<IReadOnlyDictionary<string[], string>>(
            new CommandLineConfigurationProvider(resolved).Read()
        );
    }
}
