using System;
using Annium.Core.Runtime.Types;
using Annium.Testing;
using Xunit;

namespace Annium.Serialization.Json.Tests.Converters;

/// <summary>
/// Tests for abstract type JSON converter functionality
/// </summary>
public class AbstractJsonConverterTest : TestBase
{
    /// <summary>
    /// Tests that serialization of ID-based abstract types works correctly
    /// </summary>
    [Fact]
    public void Serialization_IdPlain_Works()
    {
        // arrange
        var serializer = GetSerializer();
        var a = new IdChildA { Value = 1 };
        var b = new IdChildB { Value = 2 };
        var arr = new IdBase[] { a, b };

        // act
        var result = serializer.Serialize(arr);

        // assert
        result.Is($@"[{{""value"":1,""type"":""{a.Type}""}},{{""value"":2,""type"":""{b.Type}""}}]");
    }

    /// <summary>
    /// Tests that serialization of key-based abstract types works correctly
    /// </summary>
    [Fact]
    public void Serialization_KeyPlain_Works()
    {
        // arrange
        var serializer = GetSerializer();
        KeyBase a = new KeyChildA { Value = 1 };
        KeyBase b = new KeyChildB { Value = 2 };
        var arr = new[] { a, b };

        // act
        var result = serializer.Serialize(arr);

        // assert
        result.Is(@"[{""value"":1,""type"":""a""},{""value"":2,""type"":""b""}]");
    }

    /// <summary>
    /// Tests that serialization of interface to struct works correctly
    /// </summary>
    [Fact]
    public void Serialization_InterfaceToStruct_Works()
    {
        // arrange
        var serializer = GetSerializer();
        IValue a = new SomeValue(2);

        // act
        var result = serializer.Serialize(a);

        // assert
        result.Is(@"{""x"":2}");
    }

    /// <summary>
    /// Tests that serialization of signature-based abstract types works correctly
    /// </summary>
    [Fact]
    public void Serialization_SignaturePlain_Works()
    {
        // arrange
        var serializer = GetSerializer();
        Base a = new ChildA { A = 1 };
        Base b = new ChildB { B = 2 };
        var arr = new[] { a, b };

        // act
        var result = serializer.Serialize(arr);

        // assert
        result.Is(@"[{""a"":1},{""b"":2}]");
    }

    /// <summary>
    /// Tests that serialization of ID-based generic abstract types works correctly
    /// </summary>
    [Fact]
    public void Serialization_IdGeneric_Works()
    {
        // arrange
        var serializer = GetSerializer();
        var a = new IdChildA { Value = 1 };
        var b = new IdChildB { Value = 2 };
        IdBaseContainer<IdBase> container = new IdDataContainer<IdBase> { Items = new IdBase[] { a, b } };

        // act
        var result = serializer.Serialize(container);

        // assert
        result.Is(
            $@"{{""items"":[{{""value"":1,""type"":""{a.Type}""}},{{""value"":2,""type"":""{b.Type}""}}],""type"":""{container.Type}""}}"
        );
    }

    /// <summary>
    /// Tests that serialization of key-based generic abstract types works correctly
    /// </summary>
    [Fact]
    public void Serialization_KeyGeneric_Works()
    {
        // arrange
        var serializer = GetSerializer();
        KeyBase a = new KeyChildA { Value = 1 };
        KeyBase b = new KeyChildB { Value = 2 };
        KeyBaseContainer<KeyBase> container = new KeyDataContainer<KeyBase> { Items = new[] { a, b } };

        // act
        var result = serializer.Serialize(container);

        // assert
        result.Is(@"{""items"":[{""value"":1,""type"":""a""},{""value"":2,""type"":""b""}],""type"":""a""}");
    }

    /// <summary>
    /// Tests that serialization of signature-based generic abstract types works correctly
    /// </summary>
    [Fact]
    public void Serialization_SignatureGeneric_Works()
    {
        // arrange
        var serializer = GetSerializer();
        Base a = new ChildA { A = 1 };
        Base b = new ChildB { B = 2 };
        BaseContainer<Base> container = new DataContainer<Base> { Data = new[] { a, b } };

        // act
        var result = serializer.Serialize(container);

        // assert
        result.Is(@"{""data"":[{""a"":1},{""b"":2}]}");
    }

    /// <summary>
    /// Tests that deserialization of ID-based abstract types works correctly
    /// </summary>
    [Fact]
    public void Deserialization_IdPlain_Works()
    {
        // arrange
        var serializer = GetSerializer();
        var a = new IdChildA { Value = 1 };
        var b = new IdChildB { Value = 2 };
        var arr = new IdBase[] { a, b };
        var str = serializer.Serialize(arr);

        // act
        var result = serializer.Deserialize<IdBase[]>(str);

        // assert
        result.Has(2);
        result.At(0).As<IdChildA>().Value.Is(1);
        result.At(1).As<IdChildB>().Value.Is(2);
    }

    /// <summary>
    /// Tests that deserialization of key-based abstract types works correctly
    /// </summary>
    [Fact]
    public void Deserialization_KeyPlain_Works()
    {
        // arrange
        var serializer = GetSerializer();
        var a = new KeyChildA { Value = 1 };
        var b = new KeyChildB { Value = 2 };
        var arr = new KeyBase[] { a, b };
        var str = serializer.Serialize(arr);

        // act
        var result = serializer.Deserialize<KeyBase[]>(str);

        // assert
        result.Has(2);
        result.At(0).As<KeyChildA>().Value.Is(1);
        result.At(1).As<KeyChildB>().Value.Is(2);
    }

    /// <summary>
    /// Tests that deserialization of interface to struct works correctly
    /// </summary>
    [Fact]
    public void Deserialization_InterfaceToStruct_Works()
    {
        // arrange
        var serializer = GetSerializer();
        IValue a = new SomeValue(2);
        var str = serializer.Serialize(a);

        // act
        var result = serializer.Deserialize<IValue>(str);

        // assert
        result.As<SomeValue>().X.Is(2);
    }

    /// <summary>
    /// Tests that deserialization of signature-based abstract types works correctly
    /// </summary>
    [Fact]
    public void Deserialization_SignaturePlain_Works()
    {
        // arrange
        var serializer = GetSerializer();
        var a = new ChildA { A = 1 };
        var b = new ChildB { B = 2 };
        var arr = new Base[] { a, b };
        var str = serializer.Serialize(arr);

        // act
        var result = serializer.Deserialize<Base[]>(str);

        // assert
        result.Has(2);
        result.At(0).As<ChildA>().A.Is(1);
        result.At(1).As<ChildB>().B.Is(2);
    }

    /// <summary>
    /// Tests that deserialization of ID-based generic abstract types works correctly
    /// </summary>
    [Fact]
    public void Deserialization_IdGeneric_Works()
    {
        // arrange
        var serializer = GetSerializer();
        var a = new IdChildA { Value = 1 };
        var b = new IdChildB { Value = 2 };
        IdBaseContainer<IdBase> container = new IdDataContainer<IdBase> { Items = new IdBase[] { a, b } };
        var str = serializer.Serialize(container);

        // act
        var result = serializer.Deserialize<IdBaseContainer<IdBase>>(str);

        // assert
        var data = result.As<IdDataContainer<IdBase>>().Items;
        data.Has(2);
        data.At(0).As<IdChildA>().Value.Is(1);
        data.At(1).As<IdChildB>().Value.Is(2);
    }

    /// <summary>
    /// Tests that deserialization of key-based generic abstract types works correctly
    /// </summary>
    [Fact]
    public void Deserialization_KeyGeneric_Works()
    {
        // arrange
        var serializer = GetSerializer();
        var a = new KeyChildA { Value = 1 };
        var b = new KeyChildB { Value = 2 };
        KeyBaseContainer<KeyBase> container = new KeyDataContainer<KeyBase> { Items = new KeyBase[] { a, b } };
        var str = serializer.Serialize(container);

        // act
        var result = serializer.Deserialize<KeyBaseContainer<KeyBase>>(str);

        // assert
        var data = result.As<KeyDataContainer<KeyBase>>().Items;
        data.Has(2);
        data.At(0).As<KeyChildA>().Value.Is(1);
        data.At(1).As<KeyChildB>().Value.Is(2);
    }

    /// <summary>
    /// Tests that deserialization of signature-based generic abstract types works correctly
    /// </summary>
    [Fact]
    public void Deserialization_SignatureGeneric_Works()
    {
        // arrange
        var serializer = GetSerializer();
        Base a = new ChildA { A = 1 };
        Base b = new ChildB { B = 2 };
        BaseContainer<Base> container = new DataContainer<Base> { Data = new[] { a, b } };
        var str = serializer.Serialize(container);

        // act
        var result = serializer.Deserialize<BaseContainer<Base>>(str);

        // assert
        var data = result.As<DataContainer<Base>>().Data;
        data.Has(2);
        data.At(0).As<ChildA>().A.Is(1);
        data.At(1).As<ChildB>().B.Is(2);
    }

    /// <summary>
    /// Base class for ID-based resolution testing
    /// </summary>
    public abstract class IdBase
    {
        /// <summary>
        /// Gets the type identifier for resolution
        /// </summary>
        [ResolutionId]
        public string Type => GetType().GetIdString();
    }

    /// <summary>
    /// Test class A for ID-based resolution
    /// </summary>
    public class IdChildA : IdBase
    {
        /// <summary>
        /// Gets or sets the test value
        /// </summary>
        public int Value { get; set; }
    }

    /// <summary>
    /// Test class B for ID-based resolution
    /// </summary>
    public class IdChildB : IdBase
    {
        /// <summary>
        /// Gets or sets the test value
        /// </summary>
        public int Value { get; set; }
    }

    /// <summary>
    /// Base class for key-based resolution testing
    /// </summary>
    public abstract class KeyBase
    {
        /// <summary>
        /// Gets or sets the type key for resolution
        /// </summary>
        [ResolutionKey]
        public char Type { get; set; }
    }

    /// <summary>
    /// Test class A for key-based resolution with key 'a'
    /// </summary>
    [ResolutionKeyValue('a')]
    public class KeyChildA : KeyBase
    {
        /// <summary>
        /// Gets or sets the test value
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// Initializes a new instance of the KeyChildA class
        /// </summary>
        public KeyChildA()
        {
            Type = 'a';
        }
    }

    /// <summary>
    /// Test class B for key-based resolution with key 'b'
    /// </summary>
    [ResolutionKeyValue('b')]
    public class KeyChildB : KeyBase
    {
        /// <summary>
        /// Gets or sets the test value
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// Initializes a new instance of the KeyChildB class
        /// </summary>
        public KeyChildB()
        {
            Type = 'b';
        }
    }

    /// <summary>
    /// Base class for signature-based resolution testing
    /// </summary>
    public abstract class Base;

    /// <summary>
    /// Test class A for signature-based resolution
    /// </summary>
    public class ChildA : Base
    {
        /// <summary>
        /// Gets or sets the A property
        /// </summary>
        public int A { get; set; }
    }

    /// <summary>
    /// Test class B for signature-based resolution
    /// </summary>
    public class ChildB : Base
    {
        /// <summary>
        /// Gets or sets the B property
        /// </summary>
        public int B { get; set; }
    }

    /// <summary>
    /// Base container class for ID-based resolution testing
    /// </summary>
    /// <typeparam name="T">The type of items in the container</typeparam>
    public abstract class IdBaseContainer<T>
    {
        /// <summary>
        /// Gets the type identifier for resolution
        /// </summary>
        [ResolutionId]
        public string Type => GetType().GetIdString();
    }

    /// <summary>
    /// Data container class for ID-based resolution testing
    /// </summary>
    /// <typeparam name="T">The type of items in the container</typeparam>
    public class IdDataContainer<T> : IdBaseContainer<T>
    {
        /// <summary>
        /// Gets or sets the items in the container
        /// </summary>
        public T[] Items { get; set; } = Array.Empty<T>();
    }

    /// <summary>
    /// Demo container class for ID-based resolution testing
    /// </summary>
    /// <typeparam name="T">The type of items in the container</typeparam>
    public class IdDemoContainer<T> : IdBaseContainer<T>
    {
        /// <summary>
        /// Gets or sets the demo items in the container
        /// </summary>
        public T[] Demo { get; set; } = Array.Empty<T>();
    }

    /// <summary>
    /// Base container class for key-based resolution testing
    /// </summary>
    /// <typeparam name="T">The type of items in the container</typeparam>
    public abstract class KeyBaseContainer<T>
    {
        /// <summary>
        /// Gets or sets the type key for resolution
        /// </summary>
        [ResolutionKey]
        public char Type { get; set; }
    }

    /// <summary>
    /// Data container class for key-based resolution testing with key 'a'
    /// </summary>
    /// <typeparam name="T">The type of items in the container</typeparam>
    [ResolutionKeyValue('a')]
    public class KeyDataContainer<T> : KeyBaseContainer<T>
    {
        /// <summary>
        /// Gets or sets the items in the container
        /// </summary>
        public T[] Items { get; set; } = Array.Empty<T>();

        /// <summary>
        /// Initializes a new instance of the KeyDataContainer class
        /// </summary>
        public KeyDataContainer()
        {
            Type = 'a';
        }
    }

    /// <summary>
    /// Demo container class for key-based resolution testing with key 'b'
    /// </summary>
    /// <typeparam name="T">The type of items in the container</typeparam>
    [ResolutionKeyValue('b')]
    public class KeyDemoContainer<T> : KeyBaseContainer<T>
    {
        /// <summary>
        /// Gets or sets the demo items in the container
        /// </summary>
        public T[] Demo { get; set; } = Array.Empty<T>();

        /// <summary>
        /// Initializes a new instance of the KeyDemoContainer class
        /// </summary>
        public KeyDemoContainer()
        {
            Type = 'b';
        }
    }

    /// <summary>
    /// Base container class for signature-based resolution testing
    /// </summary>
    /// <typeparam name="T">The type of items in the container</typeparam>
    public abstract class BaseContainer<T>;

    /// <summary>
    /// Data container class for signature-based resolution testing
    /// </summary>
    /// <typeparam name="T">The type of items in the container</typeparam>
    public class DataContainer<T> : BaseContainer<T>
    {
        /// <summary>
        /// Gets or sets the data items in the container
        /// </summary>
        public T[] Data { get; set; } = Array.Empty<T>();
    }

    /// <summary>
    /// Demo container class for signature-based resolution testing
    /// </summary>
    /// <typeparam name="T">The type of items in the container</typeparam>
    public class DemoContainer<T> : BaseContainer<T>
    {
        /// <summary>
        /// Gets or sets the demo items in the container
        /// </summary>
        public T[] Demo { get; set; } = Array.Empty<T>();
    }

    /// <summary>
    /// Test value struct implementing IValue interface
    /// </summary>
    /// <param name="X">The X coordinate value</param>
    public record struct SomeValue(int X) : IValue;

    /// <summary>
    /// Interface for test value types
    /// </summary>
    public interface IValue;
}
