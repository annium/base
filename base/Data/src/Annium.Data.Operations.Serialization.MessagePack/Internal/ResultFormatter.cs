using System;
using System.Collections.Generic;
using MessagePack;
using MessagePack.Formatters;

namespace Annium.Data.Operations.Serialization.MessagePack.Internal;

/// <summary>
/// MessagePack formatter for IResult instances without data
/// </summary>
internal class ResultFormatter : IMessagePackFormatter<IResult?>
{
    /// <summary>
    /// Gets the singleton instance of the formatter
    /// </summary>
    public static IMessagePackFormatter Instance { get; } = new ResultFormatter();

    /// <summary>
    /// Deserializes an IResult from MessagePack format
    /// </summary>
    /// <param name="reader">The MessagePack reader</param>
    /// <param name="options">The serialization options</param>
    /// <returns>The deserialized result instance</returns>
    public IResult Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil())
        {
            return default!;
        }

        options.Security.DepthStep(ref reader);

        IReadOnlyCollection<string> plainErrors = Array.Empty<string>();
        IReadOnlyDictionary<string, IReadOnlyCollection<string>> labeledErrors =
            new Dictionary<string, IReadOnlyCollection<string>>();

        var count = reader.ReadArrayHeader();
        for (var i = 0; i < count; i++)
        {
            switch (i)
            {
                case 0:
                    plainErrors = Helper.ReadPlainErrors(ref reader, options);
                    break;
                case 1:
                    labeledErrors = Helper.ReadLabeledErrors(ref reader, options);
                    break;
                default:
                    reader.Skip();
                    break;
            }
        }

        reader.Depth--;

        var result = Result.New().Errors(plainErrors).Errors(labeledErrors);

        return result;
    }

    /// <summary>
    /// Serializes an IResult to MessagePack format
    /// </summary>
    /// <param name="writer">The MessagePack writer</param>
    /// <param name="value">The result instance to serialize</param>
    /// <param name="options">The serialization options</param>
    public void Serialize(ref MessagePackWriter writer, IResult? value, MessagePackSerializerOptions options)
    {
        if (value == null!)
        {
            writer.WriteNil();
            return;
        }

        writer.WriteArrayHeader(2);
        MessagePackSerializer.Serialize(ref writer, value.PlainErrors, options);
        MessagePackSerializer.Serialize(ref writer, value.LabeledErrors, options);
    }
}
