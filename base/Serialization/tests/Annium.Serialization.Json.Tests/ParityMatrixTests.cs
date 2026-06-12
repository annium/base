using System;
using System.IO;
using Annium.Core.DependencyInjection;
using Annium.Core.Runtime;
using Annium.Logging.InMemory;
using Annium.Logging.Shared;
using Annium.Serialization.Abstractions;
using Annium.Testing;
using Xunit;

namespace Annium.Serialization.Json.Tests;

/// <summary>
/// Round-trip parity tests covering the four <see cref="ISerializer{TValue}"/> surfaces of the
/// JSON package: <see cref="string"/>, <see cref="byte"/>[], <see cref="ReadOnlyMemory{T}"/> of
/// <see cref="byte"/>, and <see cref="Stream"/>. Each surface must round-trip the canonical
/// <see cref="Sample"/> record without loss.
/// </summary>
public class ParityMatrixTests
{
    /// <summary>
    /// Canonical test model used by the parity matrix.
    /// </summary>
    /// <param name="Id">Numeric identifier.</param>
    /// <param name="Name">Display name.</param>
    /// <param name="At">Timestamp with offset.</param>
    public sealed record Sample(int Id, string Name, DateTimeOffset At);

    /// <summary>
    /// Round-trip of <see cref="Sample"/> through the <c>ISerializer&lt;string&gt;</c> surface.
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
    /// Round-trip of <see cref="Sample"/> through the <see cref="ISerializer{T}"/> surface where
    /// <c>T = <see cref="byte"/>[]</c>.
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
    /// Round-trip of <see cref="Sample"/> through the <see cref="ISerializer{T}"/> surface where
    /// <c>T = <see cref="ReadOnlyMemory{T}"/> of <see cref="byte"/></c>.
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
    /// Round-trip of <see cref="Sample"/> through the <see cref="ISerializer{T}"/> surface where
    /// <c>T = <see cref="Stream"/></c>.
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
        container.AddTime().WithRealTime().SetDefault();
        container.AddSerializers().WithJson();
        container.AddLogging();
        var provider = container.BuildServiceProvider();
        provider.UseLogging(x => x.UseInMemory());
        return provider.ResolveSerializer<TValue>(Abstractions.Constants.DefaultKey, Constants.MediaType);
    }
}
