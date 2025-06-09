using Annium.Serialization.Abstractions;
using Annium.Serialization.BinaryString.Internal;
using Constants = Annium.Serialization.BinaryString.Constants;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

/// <summary>
/// Extension methods for configuring binary string serialization.
/// </summary>
public static class SerializationConfigurationBuilderExtensions
{
    /// <summary>
    /// Registers binary string serialization support for converting byte arrays to hexadecimal strings.
    /// </summary>
    /// <param name="builder">The serialization configuration builder.</param>
    /// <param name="isDefault">Whether this serializer should be used as the default for byte array to string conversions.</param>
    /// <returns>The serialization configuration builder for method chaining.</returns>
    public static ISerializationConfigurationBuilder WithBinaryString(
        this ISerializationConfigurationBuilder builder,
        bool isDefault = false
    ) => builder.Register<byte[], string, HexStringSerializer>(Constants.MediaType, isDefault);
}
