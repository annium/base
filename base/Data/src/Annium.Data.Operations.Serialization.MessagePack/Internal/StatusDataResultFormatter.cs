using System;
using System.Collections.Generic;
using MessagePack;
using MessagePack.Formatters;

namespace Annium.Data.Operations.Serialization.MessagePack.Internal;

/// <summary>
/// MessagePack formatter for IStatusResult&lt;TS, TD&gt; instances with both status and data
/// </summary>
/// <typeparam name="TS">The type of status in the result</typeparam>
/// <typeparam name="TD">The type of data in the result</typeparam>
internal class StatusDataResultFormatter<TS, TD> : IMessagePackFormatter<IStatusResult<TS, TD>?>
{
    /// <summary>
    /// Gets the singleton instance of the formatter
    /// </summary>
    public static IMessagePackFormatter Instance { get; } = new StatusDataResultFormatter<TS, TD>();

    /// <summary>
    /// Deserializes an IStatusResult&lt;TS, TD&gt; from MessagePack format
    /// </summary>
    /// <param name="reader">The MessagePack reader</param>
    /// <param name="options">The serialization options</param>
    /// <returns>The deserialized status result instance</returns>
    public IStatusResult<TS, TD> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil())
        {
            return default!;
        }

        options.Security.DepthStep(ref reader);

        var status = default(TS)!;
        var data = default(TD)!;
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
                    data = MessagePackSerializer.Deserialize<TD>(ref reader, options);
                    break;
                case 2:
                    plainErrors = Helper.ReadPlainErrors(ref reader, options);
                    break;
                case 3:
                    labeledErrors = Helper.ReadLabeledErrors(ref reader, options);
                    break;
                default:
                    reader.Skip();
                    break;
            }
        }

        reader.Depth--;

        var result = Result.Status(status, data).Errors(plainErrors).Errors(labeledErrors);

        return result;
    }

    /// <summary>
    /// Serializes an IStatusResult&lt;TS, TD&gt; to MessagePack format
    /// </summary>
    /// <param name="writer">The MessagePack writer</param>
    /// <param name="value">The status result instance to serialize</param>
    /// <param name="options">The serialization options</param>
    public void Serialize(
        ref MessagePackWriter writer,
        IStatusResult<TS, TD>? value,
        MessagePackSerializerOptions options
    )
    {
        if (value == null!)
        {
            writer.WriteNil();
            return;
        }

        writer.WriteArrayHeader(4);
        MessagePackSerializer.Serialize(ref writer, value.Status, options);
        MessagePackSerializer.Serialize(ref writer, value.Data, options);
        MessagePackSerializer.Serialize(ref writer, value.PlainErrors, options);
        MessagePackSerializer.Serialize(ref writer, value.LabeledErrors, options);
    }
}
