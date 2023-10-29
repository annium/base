using System.Collections.Generic;
using System.Security;

namespace Annium.Security;

public static class CharSequenceExtensions
{
    public static SecureString AsSecureString(this IEnumerable<char> source)
    {
        var result = new SecureString();

        foreach (var ch in source)
            result.AppendChar(ch);

        return result;
    }
}
