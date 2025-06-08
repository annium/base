using System.Collections.Generic;
using System.Linq;
using Annium.Configuration.Abstractions;
using Annium.Configuration.Tests.Lib;
using Annium.Core.DependencyInjection;
using Annium.Testing;
using Xunit;

namespace Annium.Configuration.CommandLine.Tests;

/// <summary>
/// Tests for command line configuration provider functionality.
/// </summary>
public class CommandLineConfigurationProviderTest : TestBase
{
    public CommandLineConfigurationProviderTest(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        RegisterMapper();
    }

    /// <summary>
    /// Tests that command line configuration works correctly.
    /// </summary>
    [Fact]
    public void CommandLineConfiguration_Works()
    {
        // arrange
        var args = new List<string>();
        args.AddRange("-flag");
        args.AddRange("-plain", "7");
        args.AddRange("-nullable", "3");
        args.AddRange("-array", "4", "-array", "7");
        args.AddRange("-nested.plain", "4");
        args.AddRange("-nested.array", "4", "-nested.array", "13");
        Register(c => c.AddConfiguration<Config>(x => x.AddCommandLineArgs(args.ToArray())));

        // act
        var result = Get<Config>();
        var nested = Get<Val>();

        // assert
        result.IsNotDefault();
        result.Flag.IsTrue();
        result.Plain.Is(7);
        // result.Nullable.Is(3);
        result.Array.SequenceEqual(new[] { 4, 7 }).IsTrue();
        result.Nested.Plain.IsEqual(4);
        result.Nested.Array.SequenceEqual(new[] { 4m, 13m }).IsTrue();
        result.Nested.IsEqual(nested);
        nested.Plain.Is(4);
        nested.Array.SequenceEqual(new[] { 4m, 13m }).IsTrue();
    }
}

/// <summary>
/// Extension methods for List collections.
/// </summary>
internal static class ListExtensions
{
    /// <summary>
    /// Adds a range of values to the list.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    /// <param name="list">The list to add values to.</param>
    /// <param name="values">The values to add.</param>
    public static void AddRange<T>(this List<T> list, params T[] values)
    {
        list.AddRange(values);
    }
}
