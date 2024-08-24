using System;
using System.Collections.Generic;
using Microsoft.Extensions.Primitives;

namespace Annium.Net.Base.Internal;

/// <summary>
/// Provides methods for parsing and manipulating query strings.
/// </summary>
internal static class QueryHelpers
{
    /// <summary>
    /// Parse a query string into its component key and value parts.
    /// </summary>
    /// <param name="queryString">The raw query string value, with or without the leading '?'.</param>
    /// <returns>A collection of parsed keys and values.</returns>
    public static Dictionary<string, StringValues> ParseQuery(string? queryString)
    {
        var accumulator = new KeyValueAccumulator();

        if (string.IsNullOrEmpty(queryString) || queryString == "?")
        {
            return new Dictionary<string, StringValues>();
        }

        var scanIndex = 0;
        if (queryString[0] == '?')
        {
            scanIndex = 1;
        }

        var textLength = queryString.Length;
        var equalIndex = queryString.IndexOf('=');
        if (equalIndex == -1)
        {
            equalIndex = textLength;
        }

        while (scanIndex < textLength)
        {
            var delimiterIndex = queryString.IndexOf('&', scanIndex);
            if (delimiterIndex == -1)
            {
                delimiterIndex = textLength;
            }

            if (equalIndex < delimiterIndex)
            {
                while (scanIndex != equalIndex && char.IsWhiteSpace(queryString[scanIndex]))
                {
                    ++scanIndex;
                }

                var name = queryString[scanIndex..equalIndex];
                var value = queryString.Substring(equalIndex + 1, delimiterIndex - equalIndex - 1);
                accumulator.Append(
                    Uri.UnescapeDataString(name.Replace('+', ' ')),
                    Uri.UnescapeDataString(value.Replace('+', ' '))
                );
                equalIndex = queryString.IndexOf('=', delimiterIndex);
                if (equalIndex == -1)
                {
                    equalIndex = textLength;
                }
            }
            else
            {
                if (delimiterIndex > scanIndex)
                {
                    accumulator.Append(queryString[scanIndex..delimiterIndex], string.Empty);
                }
            }

            scanIndex = delimiterIndex + 1;
        }

        return accumulator.GetResults();
    }
}
