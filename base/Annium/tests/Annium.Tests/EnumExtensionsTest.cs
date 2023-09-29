using System;
using System.ComponentModel;
using Annium.Testing;
using Xunit;

namespace Annium.Tests;

public class EnumExtensionsTest
{
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

    [Flags]
    private enum TestEnum
    {
        [Description("empty")]
        None = 0,

        [Description("a")]
        One = 1,

        [Description("b")]
        Two = 2,

        [Description("Other")]
        Three = 4,
    }
}