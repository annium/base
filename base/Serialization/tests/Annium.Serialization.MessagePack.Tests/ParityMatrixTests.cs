using System;
using System.IO;
using Annium.Core.DependencyInjection;
using Annium.Core.Runtime;
using Annium.Logging.InMemory;
using Annium.Logging.Shared;
using Annium.Serialization.Abstractions;
using Annium.Testing;
using MessagePack;
using Xunit;

namespace Annium.Serialization.MessagePack.Tests;

/// <summary>
/// Round-trip parity tests covering the four <see cref="ISerializer{TValue}"/> surfaces of the
/// MessagePack package. <see cref="string"/> uses base64 encoding of the binary payload.
/// </summary>
public class ParityMatrixTests
{
    /// <summary>
    /// Canonical test model used by the parity matrix.
    /// </summary>
    /// <param name="Id">Numeric identifier.</param>
    /// <param name="Name">Display name.</param>
    /// <param name="At">Timestamp with offset.</param>
    [MessagePackObject]
    public sealed record Sample(
        [property: Key(0)] int Id,
        [property: Key(1)] string Name,
        [property: Key(2)] DateTimeOffset At
    );

    /// <summary>
    /// Round-trip through the <c>ISerializer&lt;string&gt;</c> surface (base64-encoded binary).
    /// </summary>
    [Fact]
    public void RoundTrip_StringSurface()
    {
        var sample = new Sample(7, "alpha", new DateTimeOffset(2026, 4, 25, 12, 0, 0, TimeSpan.Zero));
        var serializer = Resolve<string>();
        var serialized = serializer.Serialize(sample);
        var deserialized = serializer.Deserialize<Sample>(serialized);
        deserialized.Is(sample);
    }

    /// <summary>
    /// Round-trip through the <see cref="ISerializer{T}"/> surface where <c>T = byte[]</c>.
    /// </summary>
    [Fact]
    public void RoundTrip_ByteArraySurface()
    {
        var sample = new Sample(7, "alpha", new DateTimeOffset(2026, 4, 25, 12, 0, 0, TimeSpan.Zero));
        var serializer = Resolve<byte[]>();
        var serialized = serializer.Serialize(sample);
        var deserialized = serializer.Deserialize<Sample>(serialized);
        deserialized.Is(sample);
    }

    /// <summary>
    /// Round-trip through the <see cref="ISerializer{T}"/> surface where <c>T = ReadOnlyMemory&lt;byte&gt;</c>.
    /// </summary>
    [Fact]
    public void RoundTrip_ReadOnlyMemoryByteSurface()
    {
        var sample = new Sample(7, "alpha", new DateTimeOffset(2026, 4, 25, 12, 0, 0, TimeSpan.Zero));
        var serializer = Resolve<ReadOnlyMemory<byte>>();
        var serialized = serializer.Serialize(sample);
        var deserialized = serializer.Deserialize<Sample>(serialized);
        deserialized.Is(sample);
    }

    /// <summary>
    /// Round-trip through the <see cref="ISerializer{T}"/> surface where <c>T = Stream</c>.
    /// </summary>
    [Fact]
    public void RoundTrip_StreamSurface()
    {
        var sample = new Sample(7, "alpha", new DateTimeOffset(2026, 4, 25, 12, 0, 0, TimeSpan.Zero));
        var serializer = Resolve<Stream>();
        using var serialized = serializer.Serialize(sample);
        var deserialized = serializer.Deserialize<Sample>(serialized);
        deserialized.Is(sample);
    }

    /// <summary>
    /// Resolves a serializer for the specified payload type using the standard test wiring.
    /// </summary>
    /// <typeparam name="TValue">Surface payload type.</typeparam>
    /// <returns>Resolved serializer instance.</returns>
    private static ISerializer<TValue> Resolve<TValue>()
    {
        var container = new ServiceContainer();
        container.AddRuntime(typeof(ParityMatrixTests).Assembly);
        container.AddSerializers().WithMessagePack(() => MessagePackSerializerOptions.Standard);
        container.AddTime().WithManagedTime().SetDefault();
        container.AddLogging();
        var provider = container.BuildServiceProvider();
        provider.UseLogging(x => x.UseInMemory());
        return provider.ResolveSerializer<TValue>(Abstractions.Constants.DefaultKey, Constants.MediaType);
    }
}
