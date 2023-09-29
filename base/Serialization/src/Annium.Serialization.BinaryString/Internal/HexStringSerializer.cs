using Annium.Serialization.Abstractions;

namespace Annium.Serialization.BinaryString.Internal;

internal class HexStringSerializer : ISerializer<byte[], string>
{
    public byte[] Deserialize(string value)
    {
        return value.FromHexStringToByteArray();
    }

    public string Serialize(byte[] value)
    {
        return value.ToHexString();
    }
}