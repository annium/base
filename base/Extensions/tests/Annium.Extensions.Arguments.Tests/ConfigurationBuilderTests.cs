using System;
using Annium.Core.DependencyInjection;
using Annium.Core.Mapper;
using Annium.Testing;
using Xunit;

namespace Annium.Extensions.Arguments.Tests;

/// <summary>
/// Tests for how a parsed command line is bound onto a configuration object: positions, allowed values,
/// the raw tail and array-valued options. This is what every command in every CLI built on this library
/// receives, and none of it was covered.
/// </summary>
public class ConfigurationBuilderTests : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationBuilderTests"/> class.
    /// </summary>
    /// <param name="outputHelper">xUnit test output helper the test host logs through.</param>
    public ConfigurationBuilderTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(container =>
        {
            container.AddMapper();
            container.AddArguments();
        });
    }

    /// <summary>
    /// Positional arguments are bound in the order they were declared, and an optional one may be absent.
    /// </summary>
    [Fact]
    public void Build_Positions_AreBoundInOrder()
    {
        // act
        var cfg = Build<PositionalConfiguration>("build", "release");

        // assert
        cfg.Command.Is("build");
        cfg.Target.Is("release");
    }

    /// <summary>
    /// A missing optional position leaves its property alone rather than failing.
    /// </summary>
    [Fact]
    public void Build_OptionalPositionAbsent_IsLeftAlone()
    {
        // act
        var cfg = Build<PositionalConfiguration>("build");

        // assert
        cfg.Command.Is("build");
        cfg.Target.Is(string.Empty);
    }

    /// <summary>
    /// A required position with nothing to bind to is a usage error naming the property.
    /// </summary>
    [Fact]
    public void Build_RequiredPositionAbsent_Throws()
    {
        // act & assert
        var error = Wrap.It(() => Build<PositionalConfiguration>()).Throws<ArgumentParseException>();
        error.Message.Contains(nameof(PositionalConfiguration.Command)).IsTrue();
    }

    /// <summary>
    /// Positions are one-based and must run without gaps; a declaration that does not is reported, with
    /// both the expected and the declared position readable in the message.
    /// </summary>
    [Fact]
    public void Build_PositionsMisdeclared_ThrowsAReadableMessage()
    {
        // act & assert - the first position is 1, so a 0 is a mistake worth naming
        var error = Wrap.It(() => Build<ZeroBasedConfiguration>("x")).Throws<ArgumentParseException>();
        error.Message.Contains("position '1'").IsTrue("the message must say which position was expected");
        error.Message.Contains("position '0'").IsTrue("and which one was declared, quoted on both sides");
    }

    /// <summary>
    /// A value outside the allowed set is rejected, and the message lists what was allowed.
    /// </summary>
    [Fact]
    public void Build_ValueOutsideAllowedValues_Throws()
    {
        // act & assert
        var error = Wrap.It(() => Build<ConstrainedConfiguration>("-mode", "sideways"))
            .Throws<ArgumentParseException>();
        error.Message.Contains("sideways").IsTrue("the message must name the rejected value");
        error.Message.Contains("up").IsTrue("and list what was allowed");
    }

    /// <summary>
    /// A value inside the allowed set binds normally.
    /// </summary>
    [Fact]
    public void Build_ValueInsideAllowedValues_Binds()
    {
        // act
        var cfg = Build<ConstrainedConfiguration>("-mode", "up");

        // assert
        cfg.Mode.Is("up");
    }

    /// <summary>
    /// Everything after the raw delimiter is handed over verbatim.
    /// </summary>
    [Fact]
    public void Build_RawTail_IsCapturedVerbatim()
    {
        // act
        var cfg = Build<RawTailConfiguration>("run", "--", "-force", "value");

        // assert
        cfg.Command.Is("run");
        cfg.Rest.Is("-force value");
    }

    /// <summary>
    /// An array option collects a repeated option, and a single occurrence still yields one element.
    /// </summary>
    [Fact]
    public void Build_ArrayOption_CollectsRepeatedAndSingle()
    {
        // act
        var many = Build<ArrayConfiguration>("-include", "a", "-include", "b");
        var one = Build<ArrayConfiguration>("-include", "a");

        // assert
        many.Include.Has(2).At(0).Is("a");
        many.Include.At(1).Is("b");
        one.Include.Has(1).At(0).Is("a");
    }

    /// <summary>
    /// An option given by its alias binds the same as one given by its name.
    /// </summary>
    [Fact]
    public void Build_OptionByAlias_Binds()
    {
        // act
        var cfg = Build<AliasedConfiguration>("-o", "dist");

        // assert
        cfg.Output.Is("dist");
    }

    /// <summary>
    /// A value that cannot be converted to the property's type is a usage error like any other, and is
    /// reported as one - not as whatever the conversion happened to throw.
    /// </summary>
    [Fact]
    public void Build_ValueOfTheWrongType_ThrowsArgumentParseException()
    {
        // act & assert
        var error = Wrap.It(() => Build<TypedConfiguration>("-count", "abc")).Throws<ArgumentParseException>();
        error.Message.Contains("abc").IsTrue("the message must name the value that could not be converted");
        error.Message.Contains(nameof(TypedConfiguration.Count)).IsTrue("and the option it was given for");
    }

    /// <summary>
    /// A value that can be converted still binds.
    /// </summary>
    [Fact]
    public void Build_ValueOfTheRightType_Binds()
    {
        // act
        var cfg = Build<TypedConfiguration>("-count", "3");

        // assert
        cfg.Count.Is(3);
    }

    /// <summary>
    /// A flag followed by a positional argument stays a flag, and the positional argument stays one. A
    /// flag never takes a value, so swallowing the next token loses both of them at once.
    /// </summary>
    [Fact]
    public void Build_FlagFollowedByAPosition_KeepsBoth()
    {
        // act
        var cfg = Build<FlagAndPositionConfiguration>("-verbose", "report.txt");

        // assert
        cfg.Verbose.IsTrue("the flag must be set");
        cfg.Path.Is("report.txt", "and the value after it must remain a position");
    }

    /// <summary>
    /// The same by alias.
    /// </summary>
    [Fact]
    public void Build_FlagByAliasFollowedByAPosition_KeepsBoth()
    {
        // act
        var cfg = Build<FlagAndPositionConfiguration>("-v", "report.txt");

        // assert
        cfg.Verbose.IsTrue("the flag must be set when given by its alias");
        cfg.Path.Is("report.txt");
    }

    /// <summary>
    /// A non-boolean option still takes the token after it.
    /// </summary>
    [Fact]
    public void Build_OptionFollowedByItsValue_StillTakesIt()
    {
        // act
        var cfg = Build<FlagAndPositionConfiguration>("-name", "world", "report.txt");

        // assert
        cfg.Name.Is("world");
        cfg.Path.Is("report.txt");
    }

    /// <summary>
    /// Builds a configuration of the given type from a command line.
    /// </summary>
    /// <typeparam name="T">The configuration type to build.</typeparam>
    /// <param name="args">The command line to bind.</param>
    /// <returns>The bound configuration.</returns>
    private T Build<T>(params string[] args)
        where T : new() => Get<Root>().ConfigurationBuilder.Build<T>(args);
}

/// <summary>
/// Configuration with a required and an optional positional argument.
/// </summary>
public class PositionalConfiguration
{
    /// <summary>
    /// Gets or sets the first positional argument.
    /// </summary>
    [Position(1)]
    public string Command { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the second, optional positional argument.
    /// </summary>
    [Position(2, isRequired: false)]
    public string Target { get; set; } = string.Empty;
}

/// <summary>
/// Configuration declaring its first position as 0, which the one-based numbering rejects.
/// </summary>
public class ZeroBasedConfiguration
{
    /// <summary>
    /// Gets or sets a positional argument declared at the wrong position.
    /// </summary>
    [Position(0)]
    public string Command { get; set; } = string.Empty;
}

/// <summary>
/// Configuration whose option accepts only a fixed set of values.
/// </summary>
public class ConstrainedConfiguration
{
    /// <summary>
    /// Gets or sets the direction to move in.
    /// </summary>
    [Option]
    [Values("up", "down")]
    public string Mode { get; set; } = string.Empty;
}

/// <summary>
/// Configuration capturing everything after the raw delimiter.
/// </summary>
public class RawTailConfiguration
{
    /// <summary>
    /// Gets or sets the command to run.
    /// </summary>
    [Position(1)]
    public string Command { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets everything given after the delimiter.
    /// </summary>
    [Raw]
    public string Rest { get; set; } = string.Empty;
}

/// <summary>
/// Configuration with an array-valued option.
/// </summary>
public class ArrayConfiguration
{
    /// <summary>
    /// Gets or sets the values to include.
    /// </summary>
    [Option]
    public string[] Include { get; set; } = Array.Empty<string>();
}

/// <summary>
/// Configuration with an aliased option.
/// </summary>
public class AliasedConfiguration
{
    /// <summary>
    /// Gets or sets the output directory.
    /// </summary>
    [Option("o")]
    public string Output { get; set; } = string.Empty;
}

/// <summary>
/// Configuration with a non-string option.
/// </summary>
public class TypedConfiguration
{
    /// <summary>
    /// Gets or sets how many.
    /// </summary>
    [Option]
    public int Count { get; set; }
}

/// <summary>
/// Configuration combining a flag, an option and an optional position.
/// </summary>
public class FlagAndPositionConfiguration
{
    /// <summary>
    /// Gets or sets whether to be verbose.
    /// </summary>
    [Option("v")]
    public bool Verbose { get; set; }

    /// <summary>
    /// Gets or sets who to greet.
    /// </summary>
    [Option]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the path to work on.
    /// </summary>
    [Position(1, isRequired: false)]
    public string Path { get; set; } = string.Empty;
}
