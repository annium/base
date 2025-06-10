using Annium.Serialization.Abstractions;

namespace Annium.Serialization.BinaryString.Internal;

/// <summary>
/// Serializer that converts between byte arrays and hexadecimal string representations.
/// </summary>
internal class HexStringSerializer : ISerializer<byte[], string>
{
    /// <summary>
    /// Deserializes a hexadecimal string to a byte array.
    /// </summary>
    /// <param name="value">The hexadecimal string to deserialize.</param>
    /// <returns>The byte array representation of the hexadecimal string.</returns>
    public byte[] Deserialize(string value)
    {
        return value.FromHexStringToByteArray();
    }

    /// <summary>
    /// Serializes a byte array to a hexadecimal string.
    /// </summary>
    /// <param name="value">The byte array to serialize.</param>
    /// <returns>The hexadecimal string representation of the byte array.</returns>
    public string Serialize(byte[] value)
    {
        return value.ToHexString();
    }
}
