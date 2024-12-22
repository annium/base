using System;
using System.Collections.Generic;
using MessagePack;
using MessagePack.Formatters;

namespace Annium.Data.Operations.Serialization.MessagePack.Internal;

internal class StatusDataResultFormatter<TS, TD> : IMessagePackFormatter<IStatusResult<TS, TD>?>
{
    public static IMessagePackFormatter Instance { get; } = new StatusDataResultFormatter<TS, TD>();

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
