using System.Collections.Generic;
using System.Linq;
using MessagePack;

namespace Annium.Data.Operations.Serialization.MessagePack.Internal;

internal static class Helper
{
    public static IReadOnlyCollection<string> ReadPlainErrors(
        ref MessagePackReader reader,
        MessagePackSerializerOptions options
    )
    {
        return MessagePackSerializer.Deserialize<string[]>(ref reader, options);
    }

    public static IReadOnlyDictionary<string, IReadOnlyCollection<string>> ReadLabeledErrors(
        ref MessagePackReader reader,
        MessagePackSerializerOptions options
    )
    {
        return MessagePackSerializer.Deserialize<Dictionary<string, string[]>>(ref reader, options)
            .ToDictionary(x => x.Key, x => (IReadOnlyCollection<string>)x.Value);
    }
}