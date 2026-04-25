using System;
using System.IO;
using Annium.Core.DependencyInjection;
using Annium.Core.Runtime;
using Annium.Serialization.Abstractions;
using Annium.Testing;
using Xunit;

namespace Annium.Serialization.Yaml.Tests;

/// <summary>
/// Round-trip parity tests covering the four <see cref="ISerializer{TValue}"/> surfaces of the
/// YAML package — all backed by UTF-8 encoding of the same YAML text.
/// </summary>
public class ParityMatrixTests
{
    /// <summary>
    /// Canonical test model used by the parity matrix. YamlDotNet requires a parameterless
    /// constructor with mutable members, so this is a class with property setters rather
    /// than a positional record. <see cref="DateTimeOffset"/> from §8.1.2 is replaced with
    /// <see cref="DateTime"/> here because YamlDotNet does not natively round-trip
    /// <c>DateTimeOffset</c> (its emitter writes the internal struct shape).
    /// </summary>
    public sealed class Sample : IEquatable<Sample>
    {
        /// <summary>Numeric identifier.</summary>
        public int Id { get; set; }

        /// <summary>Display name.</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Timestamp.</summary>
        public DateTime At { get; set; }

        /// <summary>Determines value-based equality.</summary>
        /// <param name="other">The other instance to compare.</param>
        /// <returns>True if equal; otherwise false.</returns>
        public bool Equals(Sample? other) =>
            other is not null && Id == other.Id && Name == other.Name && At.Equals(other.At);

        /// <summary>Determines value-based equality.</summary>
        /// <param name="obj">The other instance to compare.</param>
        /// <returns>True if equal; otherwise false.</returns>
        public override bool Equals(object? obj) => Equals(obj as Sample);

        /// <summary>Computes a hash code consistent with equality.</summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode() => HashCode.Combine(Id, Name, At);
    }

    /// <summary>
    /// Round-trip through the <c>ISerializer&lt;string&gt;</c> surface.
    /// </summary>
    [Fact]
    public void RoundTrip_StringSurface()
    {
        var sample = new Sample
        {
            Id = 7,
            Name = "alpha",
            At = new DateTime(2026, 4, 25, 12, 0, 0, DateTimeKind.Utc),
        };
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
        var sample = new Sample
        {
            Id = 7,
            Name = "alpha",
            At = new DateTime(2026, 4, 25, 12, 0, 0, DateTimeKind.Utc),
        };
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
        var sample = new Sample
        {
            Id = 7,
            Name = "alpha",
            At = new DateTime(2026, 4, 25, 12, 0, 0, DateTimeKind.Utc),
        };
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
        var sample = new Sample
        {
            Id = 7,
            Name = "alpha",
            At = new DateTime(2026, 4, 25, 12, 0, 0, DateTimeKind.Utc),
        };
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
        container.AddSerializers().WithYaml();
        var provider = container.BuildServiceProvider();
        return provider.ResolveSerializer<TValue>(Abstractions.Constants.DefaultKey, Constants.MediaType);
    }
}
