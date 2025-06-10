using System;
using System.Collections.Generic;
using MessagePack;
using MessagePack.Formatters;

namespace Annium.Data.Operations.Serialization.MessagePack.Internal;

/// <summary>
/// MessagePack formatter for IStatusResult&lt;TS&gt; instances with status but no data
/// </summary>
/// <typeparam name="TS">The type of status in the result</typeparam>
internal class StatusResultFormatter<TS> : IMessagePackFormatter<IStatusResult<TS>?>
{
    /// <summary>
    /// Gets the singleton instance of the formatter
    /// </summary>
    public static IMessagePackFormatter Instance { get; } = new StatusResultFormatter<TS>();

    /// <summary>
    /// Deserializes an IStatusResult&lt;TS&gt; from MessagePack format
    /// </summary>
    /// <param name="reader">The MessagePack reader</param>
    /// <param name="options">The serialization options</param>
    /// <returns>The deserialized status result instance</returns>
    public IStatusResult<TS> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil())
        {
            return default!;
        }

        options.Security.DepthStep(ref reader);

        var status = default(TS)!;
        IReadOnlyCollection<string> plainErrors = Array.Empty<string>();
        IReadOnlyDictionary<string, IReadOnlyCollection<string>> labeledErrors =
            new Dictionary<string, IReadOnlyCollection<string>>();

        var count = reader.ReadArrayHeader();
        for (var i = 0; i < count; i++)
        {
            switch (i)
            {
                case 0:
                    status = MessagePackSerializer.Deserialize<TS>(ref reader, options);
                    break;
                case 1:
                    plainErrors = Helper.ReadPlainErrors(ref reader, options);
                    break;
                case 2:
                    labeledErrors = Helper.ReadLabeledErrors(ref reader, options);
                    break;
                default:
                    reader.Skip();
                    break;
            }
        }

        reader.Depth--;

        var result = Result.Status(status).Errors(plainErrors).Errors(labeledErrors);

        return result;
    }

    /// <summary>
    /// Serializes an IStatusResult&lt;TS&gt; to MessagePack format
    /// </summary>
    /// <param name="writer">The MessagePack writer</param>
    /// <param name="value">The status result instance to serialize</param>
    /// <param name="options">The serialization options</param>
    public void Serialize(ref MessagePackWriter writer, IStatusResult<TS>? value, MessagePackSerializerOptions options)
    {
        if (value == null!)
        {
            writer.WriteNil();
            return;
        }

        writer.WriteArrayHeader(3);
        MessagePackSerializer.Serialize(ref writer, value.Status, options);
        MessagePackSerializer.Serialize(ref writer, value.PlainErrors, options);
        MessagePackSerializer.Serialize(ref writer, value.LabeledErrors, options);
    }
}
