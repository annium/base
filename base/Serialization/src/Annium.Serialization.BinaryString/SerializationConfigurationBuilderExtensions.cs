using Annium.Core.DependencyInjection;
using Annium.Serialization.Abstractions;
using Annium.Serialization.BinaryString.Internal;

namespace Annium.Serialization.BinaryString;

/// <summary>
/// Extension methods for configuring binary string serialization.
/// </summary>
public static class SerializationConfigurationBuilderExtensions
{
    /// <summary>
    /// Registers binary string serialization support for converting byte arrays to hexadecimal strings.
    /// In addition to the natural <c>ISerializer&lt;byte[], string&gt;</c> two-arity bridge, two
    /// single-arity wrappers are registered (<c>ISerializer&lt;byte[]&gt;</c> and
    /// <c>ISerializer&lt;string&gt;</c>) so that BinaryString participates in the parity matrix
    /// alongside the other serializer packages.
    /// </summary>
    /// <param name="builder">The serialization configuration builder.</param>
    /// <param name="isDefault">Whether this serializer should be used as the default for byte array to string conversions.</param>
    /// <returns>The serialization configuration builder for method chaining.</returns>
    public static ISerializationConfigurationBuilder WithBinaryString(
        this ISerializationConfigurationBuilder builder,
        bool isDefault = false
    )
    {
        var key = SerializerKey.Create(builder.Key, Constants.MediaType);
        return builder
            .Register<byte[], string, HexStringSerializer>(Constants.MediaType, isDefault)
            .Register<byte[], ByteArrayBridgeSerializer>(
                Constants.MediaType,
                isDefault,
                sp => new ByteArrayBridgeSerializer(sp.ResolveKeyed<ISerializer<byte[], string>>(key))
            )
            .Register<string, StringBridgeSerializer>(
                Constants.MediaType,
                isDefault,
                sp => new StringBridgeSerializer(sp.ResolveKeyed<ISerializer<byte[], string>>(key))
            );
    }
}
