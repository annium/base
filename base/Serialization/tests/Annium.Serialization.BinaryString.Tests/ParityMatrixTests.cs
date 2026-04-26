using Annium.Core.DependencyInjection;
using Annium.Core.Runtime;
using Annium.Serialization.Abstractions;
using Annium.Testing;
using Xunit;

namespace Annium.Serialization.BinaryString.Tests;

/// <summary>
/// Round-trip parity tests covering the two single-arity surfaces of the BinaryString package
/// (<c>ISerializer&lt;byte[]&gt;</c> and <c>ISerializer&lt;string&gt;</c>) added in T5.
/// Per §8.1.2 the canonical <c>Sample</c> model does not apply to BinaryString — it is a
/// byte[] ↔ string adapter — so these tests verify the natural byte[] / string round-trips.
/// </summary>
public class ParityMatrixTests
{
    /// <summary>
    /// Round-trip a byte array payload through the <c>ISerializer&lt;byte[]&gt;</c> surface.
    /// </summary>
    [Fact]
    public void RoundTrip_ByteArraySurface_ByteArrayPayload()
    {
        var serializer = Resolve<byte[]>();
        var data = new byte[] { 1, 2, 3, 4, 5 };
        var serialized = serializer.Serialize(data);
        var deserialized = serializer.Deserialize<byte[]>(serialized);
        deserialized.IsEqual(data);
    }

    /// <summary>
    /// Round-trip a string payload through the <c>ISerializer&lt;byte[]&gt;</c> surface.
    /// </summary>
    [Fact]
    public void RoundTrip_ByteArraySurface_StringPayload()
    {
        var serializer = Resolve<byte[]>();
        var hex = "01020304";
        var serialized = serializer.Serialize(hex);
        var deserialized = serializer.Deserialize<string>(serialized);
        deserialized.Is(hex);
    }

    /// <summary>
    /// Round-trip a byte array payload through the <c>ISerializer&lt;string&gt;</c> surface.
    /// </summary>
    [Fact]
    public void RoundTrip_StringSurface_ByteArrayPayload()
    {
        var serializer = Resolve<string>();
        var data = new byte[] { 0xAB, 0xCD, 0xEF };
        var serialized = serializer.Serialize(data);
        var deserialized = serializer.Deserialize<byte[]>(serialized);
        deserialized.IsEqual(data);
    }

    /// <summary>
    /// Round-trip a string payload through the <c>ISerializer&lt;string&gt;</c> surface.
    /// </summary>
    [Fact]
    public void RoundTrip_StringSurface_StringPayload()
    {
        var serializer = Resolve<string>();
        var hex = "AABBCC";
        var serialized = serializer.Serialize(hex);
        var deserialized = serializer.Deserialize<string>(serialized);
        deserialized.Is(hex);
    }

    /// <summary>
    /// Resolves a single-arity BinaryString serializer for the specified payload type.
    /// </summary>
    /// <typeparam name="TValue">Surface payload type — byte[] or string.</typeparam>
    /// <returns>Resolved serializer instance.</returns>
    private static ISerializer<TValue> Resolve<TValue>()
    {
        var container = new ServiceContainer();
        container.AddRuntime(typeof(ParityMatrixTests).Assembly);
        container.AddSerializers().WithBinaryString();
        var provider = container.BuildServiceProvider();
        return provider.ResolveSerializer<TValue>(Abstractions.Constants.DefaultKey, Constants.MediaType);
    }
}
