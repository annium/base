using System.Collections.Generic;
using System.Security;

namespace Annium.Security;

/// <summary>
/// Provides extension methods for converting character sequences to secure strings.
/// </summary>
public static class CharSequenceExtensions
{
    /// <summary>
    /// Converts a sequence of characters to a secure string.
    /// </summary>
    /// <param name="source">The sequence of characters to convert.</param>
    /// <returns>A secure string containing the characters from the source sequence.</returns>
    public static SecureString AsSecureString(this IEnumerable<char> source)
    {
        var result = new SecureString();

        foreach (var ch in source)
            result.AppendChar(ch);

        return result;
    }
}
