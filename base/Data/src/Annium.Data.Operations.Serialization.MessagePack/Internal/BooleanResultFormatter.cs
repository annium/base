using System;
using System.Collections.Generic;
using MessagePack;
using MessagePack.Formatters;

namespace Annium.Data.Operations.Serialization.MessagePack.Internal;

/// <summary>
/// MessagePack formatter for boolean result types.
/// </summary>
internal class BooleanResultFormatter : IMessagePackFormatter<IBooleanResult?>
{
    /// <summary>
    /// Gets the singleton instance of the formatter.
    /// </summary>
    public static IMessagePackFormatter Instance { get; } = new BooleanResultFormatter();

    /// <summary>
    /// Deserializes a boolean result from MessagePack format.
    /// </summary>
    /// <param name="reader">The MessagePack reader.</param>
    /// <param name="options">The serializer options.</param>
    /// <returns>The deserialized boolean result.</returns>
    public IBooleanResult Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil())
        {
            return default!;
        }

        options.Security.DepthStep(ref reader);

        var isSuccess = false;
        IReadOnlyCollection<string> plainErrors = Array.Empty<string>();
        IReadOnlyDictionary<string, IReadOnlyCollection<string>> labeledErrors =
            new Dictionary<string, IReadOnlyCollection<string>>();

        var count = reader.ReadArrayHeader();
        for (var i = 0; i < count; i++)
        {
            switch (i)
            {
                case 0:
                    isSuccess = reader.ReadBoolean();
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

        var result = isSuccess ? Result.Success() : Result.Failure();
        result.Errors(plainErrors).Errors(labeledErrors);

        return result;
    }

    /// <summary>
    /// Serializes a boolean result to MessagePack format.
    /// </summary>
    /// <param name="writer">The MessagePack writer.</param>
    /// <param name="value">The boolean result to serialize.</param>
    /// <param name="options">The serializer options.</param>
    public void Serialize(ref MessagePackWriter writer, IBooleanResult? value, MessagePackSerializerOptions options)
    {
        if (value == null!)
        {
            writer.WriteNil();
            return;
        }

        writer.WriteArrayHeader(3);
        writer.Write(value.IsSuccess);
        MessagePackSerializer.Serialize(ref writer, value.PlainErrors, options);
        MessagePackSerializer.Serialize(ref writer, value.LabeledErrors, options);
    }
}
