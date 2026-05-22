using System;
using System.Linq;
using System.Threading.Tasks;
using Annium.Configuration.Abstractions;
using Annium.Testing;
using Xunit;

namespace Annium.Configuration.CommandLine.Tests;

/// <summary>
/// Edge-case tests for <c>AddCommandLineArgs</c> covering the duplicate-flag exception path
/// and the null-args branch that delegates to <see cref="Environment.GetCommandLineArgs"/>.
/// </summary>
public class CommandLineSourceTests
{
    /// <summary>
    /// Passing the same flag twice raises an <see cref="Exception"/> wrapped in
    /// <see cref="AggregateException"/> by <c>BuildAsync</c>.
    /// </summary>
    [Fact]
    public async Task Read_DuplicateFlag_ThrowsException()
    {
        var container = ConfigurationFactory.CreateContainer();
        container.AddCommandLineArgs(new[] { "-flag", "-flag" });

        var ex = await Wrap.It(async () => await container.BuildAsync(TestContext.Current.CancellationToken))
            .ThrowsAsync<AggregateException>();
        ex.InnerExceptions.Has(1);
        var inner = ex.InnerExceptions[0];
        inner.Message.Contains("flag").IsTrue($"expected message to mention the flag; got: {inner.Message}");
    }

    /// <summary>
    /// When args is null, the source reads <see cref="Environment.GetCommandLineArgs"/> and
    /// must skip the executable path at index 0. The executable path is a positional argument
    /// (no leading dash) and therefore must never appear as a key in the flattened result.
    /// </summary>
    [Fact]
    public async Task LoadAsync_NullArgs_UsesEnvironmentArgsSkippingExecutable()
    {
        var container = ConfigurationFactory.CreateContainer();
        // optional: true keeps this test stable across test-host arg shapes (e.g. a
        // duplicate flag in the env would otherwise fail BuildAsync).
        container.AddCommandLineArgs(args: null, optional: true);

        await container.BuildAsync(TestContext.Current.CancellationToken);

        var executablePath = Environment.GetCommandLineArgs()[0];
        var data = container.Get();
        var hasExecutableKey = data.Keys.Any(k =>
            string.Join(".", k).Equals(executablePath, StringComparison.OrdinalIgnoreCase)
        );
        hasExecutableKey.IsFalse(
            $"executable path '{executablePath}' must not appear as a configuration key (Skip(1) on env args)"
        );
    }
}
