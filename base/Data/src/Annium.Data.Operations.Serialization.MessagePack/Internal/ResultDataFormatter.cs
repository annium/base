using System;
using System.Collections.Generic;
using MessagePack;
using MessagePack.Formatters;

namespace Annium.Data.Operations.Serialization.MessagePack.Internal;

/// <summary>
/// MessagePack formatter for IResult&lt;T&gt; instances with data
/// </summary>
/// <typeparam name="T">The type of data contained in the result</typeparam>
internal class ResultDataFormatter<T> : IMessagePackFormatter<IResult<T>?>
{
    /// <summary>
    /// Gets the singleton instance of the formatter
    /// </summary>
    public static IMessagePackFormatter Instance { get; } = new ResultDataFormatter<T>();

    /// <summary>
    /// Deserializes an IResult&lt;T&gt; from MessagePack format
    /// </summary>
    /// <param name="reader">The MessagePack reader</param>
    /// <param name="options">The serialization options</param>
    /// <returns>The deserialized result instance</returns>
    public IResult<T> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil())
        {
            return default!;
        }

        options.Security.DepthStep(ref reader);

        var data = default(T)!;
        IReadOnlyCollection<string> plainErrors = Array.Empty<string>();
        IReadOnlyDictionary<string, IReadOnlyCollection<string>> labeledErrors =
            new Dictionary<string, IReadOnlyCollection<string>>();

        var count = reader.ReadArrayHeader();
        for (var i = 0; i < count; i++)
        {
            switch (i)
            {
                case 0:
                    data = MessagePackSerializer.Deserialize<T>(ref reader, options);
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

        var result = Result.New(data).Errors(plainErrors).Errors(labeledErrors);

        return result;
    }

    /// <summary>
    /// Serializes an IResult&lt;T&gt; to MessagePack format
    /// </summary>
    /// <param name="writer">The MessagePack writer</param>
    /// <param name="value">The result instance to serialize</param>
    /// <param name="options">The serialization options</param>
    public void Serialize(ref MessagePackWriter writer, IResult<T>? value, MessagePackSerializerOptions options)
    {
        if (value == null!)
        {
            writer.WriteNil();
            return;
        }

        writer.WriteArrayHeader(3);
        MessagePackSerializer.Serialize(ref writer, value.Data, options);
        MessagePackSerializer.Serialize(ref writer, value.PlainErrors, options);
        MessagePackSerializer.Serialize(ref writer, value.LabeledErrors, options);
    }
}
