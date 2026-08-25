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
    /// A property named as an acronym is matched the same way as any other. The lexer normalises the
    /// token it reads, so the names it is compared against have to be normalised too.
    /// </summary>
    [Fact]
    public void Build_AcronymNamedFlagFollowedByAPosition_KeepsBoth()
    {
        // act
        var cfg = Build<AcronymConfiguration>("-url", "report.txt");

        // assert
        cfg.URL.IsTrue("the flag must be recognised despite its name not surviving normalisation as-is");
        cfg.Path.Is("report.txt");
    }

    /// <summary>
    /// The same holds for an acronym-named option carrying a value.
    /// </summary>
    [Fact]
    public void Build_AcronymNamedOption_Binds()
    {
        // act
        var cfg = Build<AcronymConfiguration>("-id", "42");

        // assert
        cfg.ID.Is("42");
    }

    /// <summary>
    /// Two properties claiming the same name are a wiring mistake, and are reported as one rather than
    /// silently routing a value to whichever happened to win.
    /// </summary>
    [Fact]
    public void Build_TwoPropertiesClaimingOneName_Throws()
    {
        // act & assert
        var error = Wrap.It(() => Build<CollidingConfiguration>("-output", "dist")).Throws<ArgumentParseException>();
        error.Message.Contains("Output").IsTrue("the message must name the token both properties claim");
    }

    /// <summary>
    /// A negative number is a value, not an option. Reading the leading dash as an option sign made the
    /// option and its value both lex as flags, and the number never reached the property.
    /// </summary>
    [Fact]
    public void Build_NegativeNumberValue_Binds()
    {
        // act
        var cfg = Build<NumericConfiguration>("-count", "-5");

        // assert
        cfg.Count.Is(-5);
    }

    /// <summary>
    /// The same for a negative number in a positional argument.
    /// </summary>
    [Fact]
    public void Build_NegativeNumberPosition_Binds()
    {
        // act
        var cfg = Build<NumericConfiguration>("-12");

        // assert
        cfg.Offset.Is(-12);
    }

    /// <summary>
    /// An array option collects values given under either spelling. Keying by the token rather than by the
    /// property left the two spellings in separate buckets, and only one of them survived.
    /// </summary>
    [Fact]
    public void Build_ArrayOptionByNameAndAlias_CollectsBoth()
    {
        // act
        var cfg = Build<AliasedArrayConfiguration>("-include", "a", "-i", "b");

        // assert
        cfg.Include.Has(2, "values given under either spelling belong to the same option");
    }

    /// <summary>
    /// A scalar option given twice is a usage error rather than a silently dropped value. Promoting it to
    /// a multi-option left it in a bucket the binder never reads for a non-array property.
    /// </summary>
    [Fact]
    public void Build_ScalarOptionGivenTwice_Throws()
    {
        // act & assert
        var error = Wrap.It(() => Build<AliasedConfiguration>("-output", "a", "-output", "b"))
            .Throws<ArgumentParseException>();
        error.Message.Contains("Output").IsTrue("the message must name the option given twice");
    }

    /// <summary>
    /// The same when the two occurrences use different spellings of the one option.
    /// </summary>
    [Fact]
    public void Build_ScalarOptionGivenTwiceUnderBothSpellings_Throws()
    {
        // act & assert
        Wrap.It(() => Build<AliasedConfiguration>("-output", "a", "-o", "b")).Throws<ArgumentParseException>();
    }

    /// <summary>
    /// An alias that starts with a digit cannot be told from a negative number on a command line, so it is
    /// refused when the configuration is built rather than read as a number every time.
    /// </summary>
    [Fact]
    public void Build_DigitLedAlias_Throws()
    {
        // act & assert
        var error = Wrap.It(() => Build<DigitAliasConfiguration>()).Throws<ArgumentParseException>();
        error.Message.Contains("2").IsTrue("the message must name the alias that cannot work");
    }

    /// <summary>
    /// A nullable value type binds like the type it wraps. The nullable type was handed to the mapper as-is,
    /// which has no conversion for it, so every such property failed whatever value was given.
    /// </summary>
    [Fact]
    public void Build_NullableValue_Binds()
    {
        // act
        var cfg = Build<NullableConfiguration>("-count", "5");

        // assert
        cfg.Count.Is(5);
    }

    /// <summary>
    /// A nullable option left out stays null rather than becoming the type's default.
    /// </summary>
    [Fact]
    public void Build_NullableValueAbsent_StaysNull()
    {
        // act
        var cfg = Build<NullableConfiguration>();

        // assert
        cfg.Count.IsDefault();
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

/// <summary>
/// Configuration whose property names do not survive normalisation unchanged.
/// </summary>
public class AcronymConfiguration
{
    /// <summary>
    /// Gets or sets whether to print the URL.
    /// </summary>
    [Option]
    public bool URL { get; set; }

    /// <summary>
    /// Gets or sets the identifier.
    /// </summary>
    [Option]
    public string ID { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the path to work on.
    /// </summary>
    [Position(1, isRequired: false)]
    public string Path { get; set; } = string.Empty;
}

/// <summary>
/// Configuration where one property's alias is another property's name.
/// </summary>
public class CollidingConfiguration
{
    /// <summary>
    /// Gets or sets the output directory.
    /// </summary>
    [Option]
    public string Output { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether to force, under an alias that collides with Output.
    /// </summary>
    [Option("output")]
    public bool Force { get; set; }
}

/// <summary>
/// Configuration taking numbers, which may be negative.
/// </summary>
public class NumericConfiguration
{
    /// <summary>
    /// Gets or sets how many.
    /// </summary>
    [Option]
    public int Count { get; set; }

    /// <summary>
    /// Gets or sets the offset.
    /// </summary>
    [Position(1, isRequired: false)]
    public int Offset { get; set; }
}

/// <summary>
/// Configuration with an aliased array option.
/// </summary>
public class AliasedArrayConfiguration
{
    /// <summary>
    /// Gets or sets the values to include.
    /// </summary>
    [Option("i")]
    public string[] Include { get; set; } = Array.Empty<string>();
}

/// <summary>
/// Configuration whose alias cannot be told from a negative number.
/// </summary>
public class DigitAliasConfiguration
{
    /// <summary>
    /// Gets or sets whether to do the thing.
    /// </summary>
    [Option("2")]
    public bool Two { get; set; }
}

/// <summary>
/// Configuration with a nullable value-typed option.
/// </summary>
public class NullableConfiguration
{
    /// <summary>
    /// Gets or sets how many, if given.
    /// </summary>
    [Option]
    public int? Count { get; set; }
}
