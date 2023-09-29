using System;
using Annium.Testing;
using Xunit;

namespace Annium.Tests;

public class StringExtensionsTest
{
    [Fact]
    public void IsNullOrEmpty_WorksCorrectly()
    {
        "".IsNullOrEmpty().IsTrue();
        " ".IsNullOrEmpty().IsFalse();
    }

    [Fact]
    public void IsNullOrWhiteSpace_WorksCorrectly()
    {
        "".IsNullOrWhiteSpace().IsTrue();
        " ".IsNullOrWhiteSpace().IsTrue();
    }

    [Fact]
    public void UpperFirst_WorksCorrectly()
    {
        "".UpperFirst().Is(string.Empty);
        " ".UpperFirst().Is(string.Empty);
        " ab ".UpperFirst().Is("Ab");
        " aB ".UpperFirst().Is("AB");
    }

    [Fact]
    public void LowerFirst_WorksCorrectly()
    {
        "".LowerFirst().Is(string.Empty);
        " ".LowerFirst().Is(string.Empty);
        " AB ".LowerFirst().Is("aB");
        " Ab ".LowerFirst().Is("ab");
    }

    [Fact]
    public void PascalCase_WorksCorrectly()
    {
        "".PascalCase().Is(string.Empty);
        " ".PascalCase().Is(string.Empty);
        "0".PascalCase().Is("0");
        "A B".PascalCase().Is("AB");
        " - test that__".PascalCase().Is("TestThat");
        "Foo Bar".PascalCase().Is("FooBar");
        "_foo-bar_".PascalCase().Is("FooBar");
        "FOO_BAR".PascalCase().Is("FooBar");
        "FOo_BAr".PascalCase().Is("FOoBAr");
        "andThatBAR0a00KBar0KK12312".PascalCase().Is("AndThatBar0A00KBar0Kk12312");
    }

    [Fact]
    public void CamelCase_WorksCorrectly()
    {
        "".CamelCase().Is(string.Empty);
        " ".CamelCase().Is(string.Empty);
        "A B".CamelCase().Is("aB");
        " - test that__".CamelCase().Is("testThat");
        "Foo Bar".CamelCase().Is("fooBar");
        "_foo-bar_".CamelCase().Is("fooBar");
        "FOO_BAR".CamelCase().Is("fooBar");
        "FOo_BAr".CamelCase().Is("fOoBAr");
        "andThatBAR0a00KBar0KK12312".CamelCase().Is("andThatBar0A00KBar0Kk12312");
    }

    [Fact]
    public void KebabCase_WorksCorrectly()
    {
        "".KebabCase().Is(string.Empty);
        " ".KebabCase().Is(string.Empty);
        "A B".KebabCase().Is("a-b");
        " - test that__".KebabCase().Is("test-that");
        "Foo Bar".KebabCase().Is("foo-bar");
        "_foo-bar_".KebabCase().Is("foo-bar");
        "FOO_BAR".KebabCase().Is("foo-bar");
        "FOo_BAr".KebabCase().Is("f-oo-b-ar");
        "andThatBAR0a00KBar0KK12312".KebabCase().Is("and-that-bar0-a00-k-bar0-kk12312");
    }

    [Fact]
    public void SnakeCase_WorksCorrectly()
    {
        "".SnakeCase().Is(string.Empty);
        " ".SnakeCase().Is(string.Empty);
        "A B".SnakeCase().Is("a_b");
        " - test that__".SnakeCase().Is("test_that");
        "Foo Bar".SnakeCase().Is("foo_bar");
        "_foo-bar_".SnakeCase().Is("foo_bar");
        "FOO_BAR".SnakeCase().Is("foo_bar");
        "FOo_BAr".SnakeCase().Is("f_oo_b_ar");
        "andThatBAR0a00KBar0KK12312".SnakeCase().Is("and_that_bar0_a00_k_bar0_kk12312");
    }

    [Fact]
    public void ToWords_WorksCorrectly()
    {
        "".ToWords().Has(0);
    }

    [Fact]
    public void Repeat_WorksCorrectly()
    {
        // assert
        "demo".Repeat(-2).Is("demo");
        "demo".Repeat(2).Is("demodemo");
    }

    [Fact]
    public void FromHexStringToByteArray_Null_ThrowsArgumentNullOrReturnsFalse()
    {
        // arrange
        var str = string.Empty;

        // act
        var tryResult = str.TryFromHexStringToByteArray(out var byteArray);

        // assert
        tryResult.IsFalse();
        byteArray.IsEmpty();
    }

    [Fact]
    public void FromHexStringToByteArray_InvalidFormat_ThrowsFormatOrReturnsFalse()
    {
        // arrange
        var str = "a";

        // act
        var tryResult = str.TryFromHexStringToByteArray(out var byteArray);

        // assert
        Wrap.It(() => str.FromHexStringToByteArray()).Throws<FormatException>();
        tryResult.IsFalse();
        byteArray.IsEmpty();
    }

    [Fact]
    public void FromHexStringToByteArray_InvalidChars_ThrowsOverflowOrReturnsFalse()
    {
        // arrange
        var str1 = "ag";
        var str2 = "g0";

        // act
        var tryResult1 = str1.TryFromHexStringToByteArray(out var byteArray1);
        var tryResult2 = str2.TryFromHexStringToByteArray(out var byteArray2);

        // assert
        Wrap.It(() => str1.FromHexStringToByteArray()).Throws<OverflowException>();
        Wrap.It(() => str2.FromHexStringToByteArray()).Throws<OverflowException>();
        tryResult1.IsFalse();
        tryResult2.IsFalse();
        byteArray1.IsEmpty();
        byteArray2.IsEmpty();
    }

    [Fact]
    public void FromHexStringToByteArray_Valid_Works()
    {
        // arrange
        var str = "07DC22";

        // act
        var result = str.FromHexStringToByteArray();
        var tryResult = str.TryFromHexStringToByteArray(out var byteArray);

        // assert
        result.AsSpan().SequenceEqual(new byte[] { 7, 220, 34 }).IsTrue();
        tryResult.IsTrue();
        byteArray.AsSpan().SequenceEqual(new byte[] { 7, 220, 34 }).IsTrue();
    }
}