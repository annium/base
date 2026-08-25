using Annium.Extensions.Arguments.Internal;
using Annium.Testing;
using Xunit;

namespace Annium.Extensions.Arguments.Tests;

/// <summary>
/// Tests for how a command line is taken apart. This library parses user-typed input for five separate
/// tools and had no test project at all until now, so these pin the shapes those tools depend on.
/// </summary>
public class ArgumentProcessorTests
{
    /// <summary>
    /// Bare values are positions, in the order given.
    /// </summary>
    [Fact]
    public void Compose_BareValues_AreOrderedPositions()
    {
        // act
        var result = Compose("build", "release");

        // assert
        result.Positions.Has(2).At(0).Is("build");
        result.Positions.At(1).Is("release");
        result.Flags.IsEmpty();
        result.Options.IsEmpty();
    }

    /// <summary>
    /// A dash-prefixed name followed by a value is an option; the name is normalised.
    /// </summary>
    [Fact]
    public void Compose_NameWithValue_IsOption()
    {
        // act
        var result = Compose("-output", "dist");

        // assert
        result.Options.Has(1);
        result.Options["Output"].Is("dist");
    }

    /// <summary>
    /// The same option given twice becomes a multi-option rather than the last value winning.
    /// </summary>
    [Fact]
    public void Compose_OptionRepeated_BecomesMultiOption()
    {
        // act
        var result = Compose("-include", "a", "-include", "b");

        // assert
        result.Options.IsEmpty("a repeated option is not a single-valued one");
        result.MultiOptions.Has(1);
        result.MultiOptions["Include"].Has(2);
    }

    /// <summary>
    /// A dash-prefixed name with nothing that looks like a value after it is a flag.
    /// </summary>
    [Fact]
    public void Compose_NameWithoutValue_IsFlag()
    {
        // act
        var result = Compose("build", "-force");

        // assert
        result.Flags.Has(1).At(0).Is("Force");
        result.Positions.Has(1);
    }

    /// <summary>
    /// Everything after the raw delimiter is kept verbatim rather than parsed.
    /// </summary>
    [Fact]
    public void Compose_AfterRawDelimiter_IsKeptVerbatim()
    {
        // act - the flag-looking token after -- must survive as raw text
        var result = Compose("run", "--", "-force", "value");

        // assert
        result.Positions.Has(1).At(0).Is("run");
        result.Flags.IsEmpty("nothing after the delimiter is parsed");
        result.Raw.Is("-force value");
    }

    /// <summary>
    /// A flag repeated is a usage error, and says which flag.
    /// </summary>
    [Fact]
    public void Compose_FlagRepeated_Throws()
    {
        // act & assert
        var error = Wrap.It(() => Compose("-force", "-force")).Throws<ArgumentParseException>();
        error.Message.Contains("force").IsTrue("the message must name the offending flag");
    }

    /// <summary>
    /// Composes a raw configuration from the given command line.
    /// </summary>
    /// <param name="args">The command line to parse.</param>
    /// <returns>The parsed configuration.</returns>
    private static RawConfiguration Compose(params string[] args) => new ArgumentProcessor().Compose(args);
}
