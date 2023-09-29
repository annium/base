using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace Annium;

public static class StringExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsFilled(this string? value) => !string.IsNullOrWhiteSpace(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNullOrEmpty(this string? value) => string.IsNullOrEmpty(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNullOrWhiteSpace(this string? value) => string.IsNullOrWhiteSpace(value);

    public static string UpperFirst(this string value)
    {
        value = Preprocess(value);
        if (string.IsNullOrEmpty(value))
            return value;

        return value.Substring(0, 1).ToUpperInvariant() + value.Substring(1);
    }

    public static string LowerFirst(this string value)
    {
        value = Preprocess(value);
        if (string.IsNullOrEmpty(value))
            return value;

        return value.Substring(0, 1).ToLowerInvariant() + value.Substring(1);
    }

    public static string PascalCase(this string value)
    {
        return Compound(value, PascalCaseInternal);

        static string PascalCaseInternal(string result, string word) =>
            result + word.ToLowerInvariant().UpperFirst();
    }

    public static string CamelCase(this string value)
    {
        return Compound(value, CamelCaseInternal);

        static string CamelCaseInternal(string result, string word) =>
            result + (result == string.Empty ? word.ToLowerInvariant().LowerFirst() : word.ToLowerInvariant().UpperFirst());
    }

    public static string KebabCase(this string value)
    {
        return Compound(value, KebabCaseInternal);

        static string KebabCaseInternal(string result, string word) =>
            result + (result == string.Empty ? string.Empty : "-") + word.ToLowerInvariant();
    }

    public static string SnakeCase(this string value)
    {
        return Compound(value, SnakeCaseInternal);

        static string SnakeCaseInternal(string result, string word) =>
            result + (result == string.Empty ? string.Empty : "_") + word.ToLowerInvariant();
    }

    public static string Repeat(this string value, int count)
    {
        if (string.IsNullOrEmpty(value) || count <= 0) return value;

        return new StringBuilder(value.Length * count)
            .AppendJoin(value, new string[count + 1])
            .ToString();
    }

    public static IEnumerable<string> ToWords(this string value)
    {
        value = Preprocess(value);
        if (string.IsNullOrEmpty(value)) yield break;

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

    private static readonly IReadOnlyDictionary<char, byte> HexLookup = CreateHexLookup();

    public static byte[] FromHexStringToByteArray(this string str)
    {
        if (str.Length % 2 != 0)
            throw new FormatException("Hex string must contain even chars count");

        var lookup = HexLookup;
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

    public static bool TryFromHexStringToByteArray(this string str, out byte[] byteArray)
    {
        byteArray = Array.Empty<byte>();

        if (string.IsNullOrEmpty(str) || str.Length % 2 != 0)
            return false;

        var lookup = HexLookup;
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

    private static string Compound(string value, Func<string, string, string> callback)
    {
        value = Preprocess(value);
        if (string.IsNullOrEmpty(value))
            return value;

        return ToWords(value).Aggregate(string.Empty, callback);
    }

    private static string Preprocess(string value) => value.Trim();

    private static Symbol GetSymbol(char c)
    {
        if (char.IsUpper(c)) return Symbol.Upper;
        if (char.IsLower(c)) return Symbol.Lower;
        if (char.IsDigit(c)) return Symbol.Digit;

        return Symbol.Other;
    }

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
            ['f'] = 15
        };
    }

    private enum Symbol
    {
        Upper,
        Lower,
        Digit,
        Other,
    }
}