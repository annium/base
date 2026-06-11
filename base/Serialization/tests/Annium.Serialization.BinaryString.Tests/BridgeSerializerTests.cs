using System;
using Annium.Core.DependencyInjection;
using Annium.Core.Runtime;
using Annium.Serialization.Abstractions;
using Annium.Testing;
using Xunit;

namespace Annium.Serialization.BinaryString.Tests;

/// <summary>
/// Tests for the unsupported-type NotSupportedException paths in the BinaryString bridge serializers.
/// Both ByteArrayBridgeSerializer (ISerializer&lt;byte[]&gt;) and StringBridgeSerializer
/// (ISerializer&lt;string&gt;) are obtained via DI — the internal types are not directly
/// constructible from this assembly (no InternalsVisibleTo).
/// </summary>
public class BridgeSerializerTests
{
    // -------------------------------------------------------------------------
    // ByteArrayBridgeSerializer — ISerializer<byte[]>
    // -------------------------------------------------------------------------

    /// <summary>
    /// Deserialize&lt;int&gt; on the byte-array bridge throws NotSupportedException
    /// because int is not byte[] or string.
    /// </summary>
    [Fact]
    public void ByteArrayBridge_DeserializeGeneric_UnsupportedType_ThrowsNotSupportedException()
    {
        // arrange
        var serializer = Resolve<byte[]>();

        // act / assert
        Wrap.It(() => serializer.Deserialize<int>(new byte[] { 1, 2, 3 })).Throws<NotSupportedException>();
    }

    /// <summary>
    /// Deserialize(Type, ...) on the byte-array bridge throws NotSupportedException
    /// when the runtime type is not byte[] or string.
    /// </summary>
    [Fact]
    public void ByteArrayBridge_DeserializeByType_UnsupportedType_ThrowsNotSupportedException()
    {
        // arrange
        var serializer = Resolve<byte[]>();

        // act / assert
        Wrap.It(() => serializer.Deserialize(typeof(int), new byte[] { 1, 2, 3 })).Throws<NotSupportedException>();
    }

    /// <summary>
    /// Serialize&lt;int&gt; on the byte-array bridge throws NotSupportedException
    /// because int is not byte[] or string.
    /// </summary>
    [Fact]
    public void ByteArrayBridge_SerializeGeneric_UnsupportedType_ThrowsNotSupportedException()
    {
        // arrange
        var serializer = Resolve<byte[]>();

        // act / assert
        Wrap.It(() => serializer.Serialize(42)).Throws<NotSupportedException>();
    }

    /// <summary>
    /// Serialize(Type, ...) on the byte-array bridge throws NotSupportedException
    /// when the runtime type is not byte[] or string.
    /// </summary>
    [Fact]
    public void ByteArrayBridge_SerializeByType_UnsupportedType_ThrowsNotSupportedException()
    {
        // arrange
        var serializer = Resolve<byte[]>();

        // act / assert
        Wrap.It(() => serializer.Serialize(typeof(int), 42)).Throws<NotSupportedException>();
    }

    // -------------------------------------------------------------------------
    // StringBridgeSerializer — ISerializer<string>
    // -------------------------------------------------------------------------

    /// <summary>
    /// Deserialize&lt;int&gt; on the string bridge throws NotSupportedException
    /// because int is not string or byte[].
    /// </summary>
    [Fact]
    public void StringBridge_DeserializeGeneric_UnsupportedType_ThrowsNotSupportedException()
    {
        // arrange
        var serializer = Resolve<string>();

        // act / assert
        Wrap.It(() => serializer.Deserialize<int>("0102")).Throws<NotSupportedException>();
    }

    /// <summary>
    /// Deserialize(Type, ...) on the string bridge throws NotSupportedException
    /// when the runtime type is not string or byte[].
    /// </summary>
    [Fact]
    public void StringBridge_DeserializeByType_UnsupportedType_ThrowsNotSupportedException()
    {
        // arrange
        var serializer = Resolve<string>();

        // act / assert
        Wrap.It(() => serializer.Deserialize(typeof(int), "0102")).Throws<NotSupportedException>();
    }

    /// <summary>
    /// Serialize&lt;int&gt; on the string bridge throws NotSupportedException
    /// because int is not string or byte[].
    /// </summary>
    [Fact]
    public void StringBridge_SerializeGeneric_UnsupportedType_ThrowsNotSupportedException()
    {
        // arrange
        var serializer = Resolve<string>();

        // act / assert
        Wrap.It(() => serializer.Serialize(42)).Throws<NotSupportedException>();
    }

    /// <summary>
    /// Serialize(Type, ...) on the string bridge throws NotSupportedException
    /// when the runtime type is not string or byte[].
    /// </summary>
    [Fact]
    public void StringBridge_SerializeByType_UnsupportedType_ThrowsNotSupportedException()
    {
        // arrange
        var serializer = Resolve<string>();

        // act / assert
        Wrap.It(() => serializer.Serialize(typeof(int), 42)).Throws<NotSupportedException>();
    }

    // -------------------------------------------------------------------------
    // Helper
    // -------------------------------------------------------------------------

    /// <summary>
    /// Resolves a single-arity BinaryString bridge serializer for the specified surface type.
    /// </summary>
    /// <typeparam name="TValue">Surface type — byte[] or string.</typeparam>
    /// <returns>The resolved bridge serializer instance.</returns>
    private static ISerializer<TValue> Resolve<TValue>()
    {
        var container = new ServiceContainer();
        container.AddRuntime(typeof(BridgeSerializerTests).Assembly);
        container.AddSerializers().WithBinaryString();
        var provider = container.BuildServiceProvider();
        return provider.ResolveSerializer<TValue>(Abstractions.Constants.DefaultKey, Constants.MediaType);
    }
}
