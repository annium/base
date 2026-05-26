using System.Linq;
using System.Threading.Tasks;
using Annium.Configuration.Abstractions;
using Annium.Configuration.Tests.Lib;
using Annium.Core.DependencyInjection;
using Annium.Testing;
using Xunit;

namespace Annium.Configuration.CommandLine.Tests;

/// <summary>
/// Tests for the third-and-later occurrence path of a repeated command-line option key.
/// Covers the <c>multiOptions.ContainsKey(name)</c> branch in
/// <c>CommandLineConfigurationProvider.Read()</c> that is reached only when the same
/// option appears three or more times.
/// </summary>
public class CommandLineMultiOptionTests
{
    private static ServiceContainer CreateContainer() => TestContainerFactory.Create();

    /// <summary>
    /// When the same option key is supplied three times on the command line, all three
    /// values are collected and the resolved array contains all three entries in order.
    /// The third occurrence exercises the <c>multiOptions.ContainsKey(name)</c> Add path
    /// that is not covered when the key appears only twice.
    /// </summary>
    [Fact]
    public async Task AddCommandLineArgs_OptionRepeatedThreeTimes_AllValuesCollected()
    {
        var container = CreateContainer();
        var args = new[] { "-array", "4", "-array", "7", "-array", "10" };

        await container.AddConfigurationAsync<Config>(
            x => x.AddCommandLineArgs(args),
            TestContext.Current.CancellationToken
        );

        var sp = container.BuildServiceProvider();
        var resolved = sp.Resolve<Config>();

        resolved.Array.SequenceEqual(new[] { 4, 7, 10 }).IsTrue();
    }
}
