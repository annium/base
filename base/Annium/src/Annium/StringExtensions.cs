using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace Annium;

/// <summary>
/// Provides extension methods for string manipulation and formatting.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Determines whether the string is filled (not null, empty, or whitespace).
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <returns>true if the string is filled; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsFilled([NotNullWhen(true)] this string? value) => !string.IsNullOrWhiteSpace(value);

    /// <summary>
    /// Determines whether the string is null or empty.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <returns>true if the string is null or empty; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNullOrEmpty([NotNullWhen(false)] this string? value) => string.IsNullOrEmpty(value);

    /// <summary>
    /// Determines whether the string is null, empty, or consists only of white-space characters.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <returns>true if the string is null, empty, or consists only of white-space characters; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNullOrWhiteSpace([NotNullWhen(false)] this string? value) => string.IsNullOrWhiteSpace(value);

    /// <summary>
    /// Converts the first character of the string to uppercase.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <returns>A string with the first character converted to uppercase.</returns>
    public static string UpperFirst(this string value)
    {
        value = Preprocess(value);
        if (string.IsNullOrEmpty(value))
            return value;

        return value[..1].ToUpperInvariant() + value[1..];
    }

    /// <summary>
    /// Converts the first character of the string to lowercase.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <returns>A string with the first character converted to lowercase.</returns>
    public static string LowerFirst(this string value)
    {
        value = Preprocess(value);
        if (string.IsNullOrEmpty(value))
            return value;

        return value[..1].ToLowerInvariant() + value[1..];
    }

    /// <summary>
    /// Converts the string to Pascal case (e.g., "helloWorld" becomes "HelloWorld").
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <returns>A string in Pascal case format.</returns>
    public static string PascalCase(this string value)
    {
        return Compound(value, PascalCaseInternal);

        static string PascalCaseInternal(string result, string word) => result + word.ToLowerInvariant().UpperFirst();
    }

    /// <summary>
    /// Converts the string to camel case (e.g., "HelloWorld" becomes "helloWorld").
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <returns>A string in camel case format.</returns>
    public static string CamelCase(this string value)
    {
        return Compound(value, CamelCaseInternal);

        static string CamelCaseInternal(string result, string word) =>
            result
            + (result == string.Empty ? word.ToLowerInvariant().LowerFirst() : word.ToLowerInvariant().UpperFirst());
    }

    /// <summary>
    /// Converts the string to kebab case (e.g., "helloWorld" becomes "hello-world").
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <returns>A string in kebab case format.</returns>
    public static string KebabCase(this string value)
    {
        return Compound(value, KebabCaseInternal);

        static string KebabCaseInternal(string result, string word) =>
            result + (result == string.Empty ? string.Empty : "-") + word.ToLowerInvariant();
    }

    /// <summary>
    /// Converts the string to snake case (e.g., "helloWorld" becomes "hello_world").
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <returns>A string in snake case format.</returns>
    public static string SnakeCase(this string value)
    {
        return Compound(value, SnakeCaseInternal);

        static string SnakeCaseInternal(string result, string word) =>
            result + (result == string.Empty ? string.Empty : "_") + word.ToLowerInvariant();
    }

    /// <summary>
    /// Repeats the string a specified number of times.
    /// </summary>
    /// <param name="value">The string to repeat.</param>
    /// <param name="count">The number of times to repeat the string.</param>
    /// <returns>A string that is the result of repeating the input string the specified number of times.</returns>
    public static string Repeat(this string value, int count)
    {
        if (string.IsNullOrEmpty(value) || count <= 0)
            return value;

        return new StringBuilder(value.Length * count).AppendJoin(value, new string[count + 1]).ToString();
    }

    /// <summary>
    /// Splits the string into words based on case changes and special characters.
    /// </summary>
    /// <param name="value">The string to split.</param>
    /// <returns>An enumerable of words extracted from the string.</returns>
    public static IEnumerable<string> ToWords(this string value)
    {
        value = Preprocess(value);
        if (string.IsNullOrEmpty(value))
            yield break;

        var pos = Symbol.Other;
        var from = 0;
        for (var i = 0; i < value.Length; i++)
        {
            var s = GetSymbol(value[i]);

            switch (pos)
            {
                case Symbol.Upper:
                    // if current is upper and next is lower - end here
                    if (s == Symbol.Upper)
                    {
                        if (value.Length - i > 1 && GetSymbol(value[i + 1]) == Symbol.Lower)
                            yield return End(i, s);
                    }
                    else if (s == Symbol.Lower || s == Symbol.Digit)
                        pos = s;
                    else
                        yield return End(i, s);

                    break;
                case Symbol.Lower:
                    if (s == Symbol.Upper || s == Symbol.Other)
                        yield return End(i, s);
                    else if (s == Symbol.Digit)
                        pos = Symbol.Digit;
                    break;
                case Symbol.Digit:
                    if (s != Symbol.Digit)
                        yield return End(i, s);
                    break;
                default:
                    // check if we can enter word
                    if (s == Symbol.Upper || s == Symbol.Lower || s == Symbol.Digit)
                    {
                        from = i;
                        pos = s;
                    }

                    break;
            }
        }

        if (pos != Symbol.Other)
            yield return End(value.Length, Symbol.Other);

        string End(int i, Symbol s)
        {
            pos = s;
            var result = value[from..i];
            from = i;

            return result;
        }
    }

    #region HexString

    /// <summary>
    /// A lookup table for hexadecimal characters.
    /// </summary>
    private static readonly IReadOnlyDictionary<char, byte> _hexLookup = CreateHexLookup();

    /// <summary>
    /// Converts a hexadecimal string to a byte array.
    /// </summary>
    /// <param name="str">The hexadecimal string to convert.</param>
    /// <returns>A byte array containing the converted values.</returns>
    /// <exception cref="FormatException">Thrown when the string length is not even.</exception>
    /// <exception cref="OverflowException">Thrown when the string contains invalid hexadecimal characters.</exception>
    public static byte[] FromHexStringToByteArray(this string str)
    {
        if (str.Length % 2 != 0)
            throw new FormatException("Hex string must contain even chars count");

        var lookup = _hexLookup;
        var byteArray = new byte[str.Length / 2];
        for (var i = 0; i < str.Length; i += 2)
        {
            var c1 = str[i];
            var c2 = str[i + 1];

            if (!lookup.TryGetValue(c1, out var b1))
                throw new OverflowException($"{c1} is not a valid hex character");

            if (!lookup.TryGetValue(c2, out var b2))
                throw new OverflowException($"{c2} is not a valid hex character");

            byteArray[i / 2] = (byte)((b1 << 4) + b2);
        }

        return byteArray;
    }

    /// <summary>
    /// Attempts to convert a hexadecimal string to a byte array.
    /// </summary>
    /// <param name="str">The hexadecimal string to convert.</param>
    /// <param name="byteArray">When this method returns, contains the converted byte array if successful; otherwise, an empty array.</param>
    /// <returns>true if the conversion was successful; otherwise, false.</returns>
    public static bool TryFromHexStringToByteArray(this string str, out byte[] byteArray)
    {
        byteArray = Array.Empty<byte>();

        if (string.IsNullOrEmpty(str) || str.Length % 2 != 0)
            return false;

        var lookup = _hexLookup;
        var array = new byte[str.Length / 2];
        for (var i = 0; i < str.Length; i += 2)
        {
            var c1 = str[i];
            var c2 = str[i + 1];

            if (lookup.TryGetValue(c1, out var b1) && lookup.TryGetValue(c2, out var b2))
                array[i / 2] = (byte)((b1 << 4) + b2);
            else
                return false;
        }

        byteArray = array;

        return true;
    }

    #endregion

    #region Like

    /// <summary>
    /// Compares the string against a given pattern.
    /// </summary>
    /// <param name="str">The string.</param>
    /// <param name="pattern">The pattern to match, where "*" means any sequence of characters, and "?" means any single character.</param>
    /// <returns><c>true</c> if the string matches the given pattern; otherwise <c>false</c>.</returns>
    public static bool IsLike(this string str, string pattern)
    {
        var rePattern = "^" + Regex.Escape(pattern).Replace(@"\*", ".*").Replace(@"\?", ".") + "$";
        return new Regex(rePattern, RegexOptions.IgnoreCase | RegexOptions.Singleline).IsMatch(str);
    }

    #endregion

    /// <summary>
    /// Applies a compound operation to a string using a callback function.
    /// </summary>
    /// <param name="value">The string to process.</param>
    /// <param name="callback">The callback function to apply to each word.</param>
    /// <returns>The processed string.</returns>
    private static string Compound(string value, Func<string, string, string> callback)
    {
        value = Preprocess(value);
        if (string.IsNullOrEmpty(value))
            return value;

        return ToWords(value).Aggregate(string.Empty, callback);
    }

    /// <summary>
    /// Preprocesses a string by trimming whitespace.
    /// </summary>
    /// <param name="value">The string to preprocess.</param>
    /// <returns>The preprocessed string.</returns>
    private static string Preprocess(string value) => value.Trim();

    /// <summary>
    /// Gets the symbol type of a character.
    /// </summary>
    /// <param name="c">The character to check.</param>
    /// <returns>The symbol type of the character.</returns>
    private static Symbol GetSymbol(char c)
    {
        if (char.IsUpper(c))
            return Symbol.Upper;
        if (char.IsLower(c))
            return Symbol.Lower;
        if (char.IsDigit(c))
            return Symbol.Digit;

        return Symbol.Other;
    }

    /// <summary>
    /// Creates a lookup table for hexadecimal characters.
    /// </summary>
    /// <returns>A dictionary mapping hexadecimal characters to their byte values.</returns>
    private static IReadOnlyDictionary<char, byte> CreateHexLookup()
    {
        return new Dictionary<char, byte>
        {
            ['0'] = 0,
            ['1'] = 1,
            ['2'] = 2,
            ['3'] = 3,
            ['4'] = 4,
            ['5'] = 5,
            ['6'] = 6,
            ['7'] = 7,
            ['8'] = 8,
            ['9'] = 9,
            ['A'] = 10,
            ['B'] = 11,
            ['C'] = 12,
            ['D'] = 13,
            ['E'] = 14,
            ['F'] = 15,
            ['a'] = 10,
            ['b'] = 11,
            ['c'] = 12,
            ['d'] = 13,
            ['e'] = 14,
            ['f'] = 15,
        };
    }

    /// <summary>
    /// Represents the type of a character in a string.
    /// </summary>
    private enum Symbol
    {
        /// <summary>
        /// An uppercase letter.
        /// </summary>
        Upper,

        /// <summary>
        /// A lowercase letter.
        /// </summary>
        Lower,

        /// <summary>
        /// A digit.
        /// </summary>
        Digit,

        /// <summary>
        /// Any other character.
        /// </summary>
        Other,
    }
}
