using System;
using System.Collections.Generic;
using MessagePack;
using MessagePack.Formatters;

namespace Annium.Data.Operations.Serialization.MessagePack.Internal;

/// <summary>
/// MessagePack formatter for boolean result data types.
/// </summary>
/// <typeparam name="T">The data type.</typeparam>
internal class BooleanResultDataFormatter<T> : IMessagePackFormatter<IBooleanResult<T>?>
{
    /// <summary>
    /// Gets the singleton instance of the formatter.
    /// </summary>
    public static IMessagePackFormatter Instance { get; } = new BooleanResultDataFormatter<T>();

    /// <summary>
    /// Deserializes a boolean result data from MessagePack format.
    /// </summary>
    /// <param name="reader">The MessagePack reader.</param>
    /// <param name="options">The serializer options.</param>
    /// <returns>The deserialized boolean result data.</returns>
    public IBooleanResult<T> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil())
        {
            return default!;
        }

        options.Security.DepthStep(ref reader);

        var isSuccess = false;
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
                    isSuccess = reader.ReadBoolean();
                    break;
                case 1:
                    data = MessagePackSerializer.Deserialize<T>(ref reader, options);
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

        var result = isSuccess ? Result.Success(data) : Result.Failure(data);
        result.Errors(plainErrors).Errors(labeledErrors);

        return result;
    }

    /// <summary>
    /// Serializes a boolean result data to MessagePack format.
    /// </summary>
    /// <param name="writer">The MessagePack writer.</param>
    /// <param name="value">The boolean result data to serialize.</param>
    /// <param name="options">The serializer options.</param>
    public void Serialize(ref MessagePackWriter writer, IBooleanResult<T>? value, MessagePackSerializerOptions options)
    {
        if (value == null!)
        {
            writer.WriteNil();
            return;
        }

        writer.WriteArrayHeader(4);
        writer.Write(value.IsSuccess);
        MessagePackSerializer.Serialize(ref writer, value.Data, options);
        MessagePackSerializer.Serialize(ref writer, value.PlainErrors, options);
        MessagePackSerializer.Serialize(ref writer, value.LabeledErrors, options);
    }
}
