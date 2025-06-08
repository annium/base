using System.Collections.Generic;
using System.Linq;
using MessagePack;

namespace Annium.Data.Operations.Serialization.MessagePack.Internal;

/// <summary>
/// Helper methods for MessagePack operations.
/// </summary>
internal static class Helper
{
    /// <summary>
    /// Reads plain errors from MessagePack format.
    /// </summary>
    /// <param name="reader">The MessagePack reader.</param>
    /// <param name="options">The serializer options.</param>
    /// <returns>The collection of plain errors.</returns>
    public static IReadOnlyCollection<string> ReadPlainErrors(
        ref MessagePackReader reader,
        MessagePackSerializerOptions options
    )
    {
        return MessagePackSerializer.Deserialize<string[]>(ref reader, options);
    }

    /// <summary>
    /// Reads labeled errors from MessagePack format.
    /// </summary>
    /// <param name="reader">The MessagePack reader.</param>
    /// <param name="options">The serializer options.</param>
    /// <returns>The dictionary of labeled errors.</returns>
    public static IReadOnlyDictionary<string, IReadOnlyCollection<string>> ReadLabeledErrors(
        ref MessagePackReader reader,
        MessagePackSerializerOptions options
    )
    {
        return MessagePackSerializer
            .Deserialize<Dictionary<string, string[]>>(ref reader, options)
            .ToDictionary(x => x.Key, x => (IReadOnlyCollection<string>)x.Value);
    }
}
