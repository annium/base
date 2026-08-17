using System;
using System.ComponentModel;
using Annium.Testing;
using Xunit;

namespace Annium.Tests;

/// <summary>
/// Contains unit tests for enum extension methods.
/// </summary>
public class EnumExtensionsTest
{
    /// <summary>
    /// Verifies that parsing enum values from strings without a default value works correctly.
    /// </summary>
    [Fact]
    public void ParseEnum_StringNoDefault_Works()
    {
        // arrange
        var name = "one";
        var desc = "A";
        var value = "1";
        var invalid = "5";

        // assert
        name.ParseEnum<TestEnum>().Is(TestEnum.One);
        desc.ParseEnum<TestEnum>().Is(TestEnum.One);
        value.ParseEnum<TestEnum>().Is(TestEnum.One);
        Wrap.It(() => invalid.ParseEnum<TestEnum>()).Throws<ArgumentException>();
    }

    /// <summary>
    /// Verifies that parsing enum values from numeric values without a default value works correctly.
    /// </summary>
    [Fact]
    public void ParseEnum_ValueNoDefault_Works()
    {
        // arrange
        var a = 1;
        var b = 3;
        var c = 1m;
        var d = 5m;
        var e = 8;

        // assert
        a.ParseEnum<TestEnum>().Is(TestEnum.One);
        b.ParseEnum<TestEnum>().Is(TestEnum.One | TestEnum.Two);
        c.ParseEnum<TestEnum>().Is(TestEnum.One);
        d.ParseEnum<TestEnum>().Is(TestEnum.One | TestEnum.Three);
        Wrap.It(() => e.ParseEnum<TestEnum>()).Throws<ArgumentException>();
    }

    /// <summary>
    /// Verifies that parsing flag values without a default value works correctly.
    /// </summary>
    [Fact]
    public void ParseFlags_NoDefault_Works()
    {
        // arrange
        var valid = "one | b";
        var invalid = "5, two";

        // assert
        valid.ParseFlags<TestEnum>("|").Is(TestEnum.One | TestEnum.Two);
        string.Empty.ParseFlags<TestEnum>("|").Is(TestEnum.None);
        Wrap.It(() => invalid.ParseFlags<TestEnum>(",")).Throws<ArgumentException>();
    }

    /// <summary>
    /// Verifies that parsing enum values from strings with a default value works correctly.
    /// </summary>
    [Fact]
    public void ParseEnum_StringDefault_Works()
    {
        // arrange
        var name = "one";
        var desc = "A";
        var value = "1";
        var invalid = "5";

        // assert
        name.ParseEnum(TestEnum.None).Is(TestEnum.One);
        desc.ParseEnum(TestEnum.None).Is(TestEnum.One);
        value.ParseEnum(TestEnum.None).Is(TestEnum.One);
        invalid.ParseEnum(TestEnum.None).Is(TestEnum.None);
    }

    /// <summary>
    /// Verifies that parsing enum values from numeric values with a default value works correctly.
    /// </summary>
    [Fact]
    public void ParseEnum_ValueDefault_Works()
    {
        // arrange
        var a = 1;
        var b = 3;
        var c = 1m;
        var d = 5m;
        var e = 8;

        // assert
        a.ParseEnum(TestEnum.None).Is(TestEnum.One);
        b.ParseEnum(TestEnum.None).Is(TestEnum.One | TestEnum.Two);
        c.ParseEnum(TestEnum.None).Is(TestEnum.One);
        d.ParseEnum(TestEnum.None).Is(TestEnum.One | TestEnum.Three);
        e.ParseEnum(TestEnum.None).Is(TestEnum.None);
    }

    /// <summary>
    /// Verifies that parsing flag values with a default value works correctly.
    /// </summary>
    [Fact]
    public void ParseFlags_Default_Works()
    {
        // arrange
        var valid = "one | b";
        var invalid = "5, two";

        // assert
        valid.ParseFlags("|", TestEnum.One).Is(TestEnum.One | TestEnum.Two);
        invalid.ParseFlags(",", TestEnum.One).Is(TestEnum.One | TestEnum.Two);
    }

    /// <summary>
    /// Verifies that flags enumeration works.
    /// </summary>
    [Fact]
    public void EnumerateFlags_FlagsEnum_Works()
    {
        // arrange
        var value = TestEnum.One | TestEnum.Two;

        // assert
        var result = value.EnumerateFlags();
        result.Has(3);
        result.At(0).Is(TestEnum.None);
        result.At(1).Is(TestEnum.One);
        result.At(2).Is(TestEnum.Two);
    }

    /// <summary>
    /// Verifies that non-flags enumeration works.
    /// </summary>
    [Fact]
    public void EnumerateFlags_NonFlagsEnum_Works()
    {
        // arrange
        var value = NonFlagsSimpleEnum.One;

        // assert
        var result = value.EnumerateFlags();
        result.Has(2);
        result.At(0).Is(NonFlagsSimpleEnum.None);
        result.At(1).Is(NonFlagsSimpleEnum.One);
    }

    /// <summary>
    /// Plain (non-<see cref="System.FlagsAttribute"/>) enum used to check that flag enumeration
    /// falls back to the single matching member.
    /// </summary>
    private enum NonFlagsSimpleEnum
    {
        /// <summary>No value.</summary>
        None = 0,

        /// <summary>First value.</summary>
        One = 1,

        /// <summary>Second value.</summary>
        Two = 2,
    }

    /// <summary>
    /// Flags enum with <see cref="System.ComponentModel.DescriptionAttribute"/>-annotated members,
    /// used to exercise description and flag-splitting extensions.
    /// </summary>
    [Flags]
    private enum TestEnum
    {
        /// <summary>No flag set.</summary>
        [Description("empty")]
        None = 0,

        /// <summary>First flag.</summary>
        [Description("a")]
        One = 1,

        /// <summary>Second flag.</summary>
        [Description("b")]
        Two = 2,

        /// <summary>Third flag, described with a non-lowercase description.</summary>
        [Description("Other")]
        Three = 4,
    }
}
