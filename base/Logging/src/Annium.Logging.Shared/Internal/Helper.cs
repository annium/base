using System;
using System.Collections.Generic;
using System.Text;

namespace Annium.Logging.Shared.Internal;

internal static class Helper
{
    public static (string, IReadOnlyDictionary<string, object?>) Process(string messageTemplate, IReadOnlyList<object?> dataItems)
    {
        var depth = 0;
        var keyIndex = -1;
        var itemIndex = -1;
        var sb = new StringBuilder();
        var data = new Dictionary<string, object?>();

        for (var i = 0; i < messageTemplate.Length; i++)
        {
            var ch = messageTemplate[i];
            switch (ch)
            {
                case '{':
                    // ensure not opening nested template var
                    if (keyIndex == -1)
                        keyIndex = i;
                    depth++;
                    break;
                case '}':
                    depth--;
                    if (depth != 0)
                        break;

                    var key = messageTemplate.AsSpan(keyIndex + 1, i - keyIndex - 1).ToString();
                    keyIndex = -1;

                    ++itemIndex;
                    if (dataItems.Count > itemIndex)
                        sb.Append(data[key] = dataItems[itemIndex]);
                    else
                    {
                        sb.Append('{');
                        sb.Append(key);
                        sb.Append('}');
                    }

                    break;
                default:
                    // if not inside template var name - just add char
                    if (keyIndex == -1)
                        sb.Append(ch);
                    break;
            }
        }

        var extra = dataItems.Count - itemIndex - 1;
        if (extra > 0)
            throw new InvalidOperationException($"Unexpected {extra} data item(s). Template: {messageTemplate}.");

        return (sb.ToString(), data);
    }
}