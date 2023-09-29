using System;
using Annium.Core.Runtime.Types;
using Annium.Testing;
using Xunit;

namespace Annium.Serialization.Json.Tests.Converters;

public class AbstractJsonConverterTest : TestBase
{
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
        result.Is($@"{{""items"":[{{""value"":1,""type"":""{a.Type}""}},{{""value"":2,""type"":""{b.Type}""}}],""type"":""{container.Type}""}}");
    }

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

    public abstract class IdBase
    {
        [ResolutionId]
        public string Type => GetType().GetIdString();
    }

    public class IdChildA : IdBase
    {
        public int Value { get; set; }
    }

    public class IdChildB : IdBase
    {
        public int Value { get; set; }
    }

    public abstract class KeyBase
    {
        [ResolutionKey]
        public char Type { get; set; }
    }

    [ResolutionKeyValue('a')]
    public class KeyChildA : KeyBase
    {
        public int Value { get; set; }

        public KeyChildA()
        {
            Type = 'a';
        }
    }

    [ResolutionKeyValue('b')]
    public class KeyChildB : KeyBase
    {
        public int Value { get; set; }

        public KeyChildB()
        {
            Type = 'b';
        }
    }

    public abstract class Base
    {
    }

    public class ChildA : Base
    {
        public int A { get; set; }
    }

    public class ChildB : Base
    {
        public int B { get; set; }
    }

    public abstract class IdBaseContainer<T>
    {
        [ResolutionId]
        public string Type => GetType().GetIdString();
    }

    public class IdDataContainer<T> : IdBaseContainer<T>
    {
        public T[] Items { get; set; } = Array.Empty<T>();
    }

    public class IdDemoContainer<T> : IdBaseContainer<T>
    {
        public T[] Demo { get; set; } = Array.Empty<T>();
    }

    public abstract class KeyBaseContainer<T>
    {
        [ResolutionKey]
        public char Type { get; set; }
    }

    [ResolutionKeyValue('a')]
    public class KeyDataContainer<T> : KeyBaseContainer<T>
    {
        public T[] Items { get; set; } = Array.Empty<T>();

        public KeyDataContainer()
        {
            Type = 'a';
        }
    }

    [ResolutionKeyValue('b')]
    public class KeyDemoContainer<T> : KeyBaseContainer<T>
    {
        public T[] Demo { get; set; } = Array.Empty<T>();

        public KeyDemoContainer()
        {
            Type = 'b';
        }
    }

    public abstract class BaseContainer<T>
    {
    }

    public class DataContainer<T> : BaseContainer<T>
    {
        public T[] Data { get; set; } = Array.Empty<T>();
    }

    public class DemoContainer<T> : BaseContainer<T>
    {
        public T[] Demo { get; set; } = Array.Empty<T>();
    }

    public record struct SomeValue(int X) : IValue;

    public interface IValue
    {
    }
}