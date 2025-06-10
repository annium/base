using System;
using System.Collections.Generic;
using Annium.Core.Runtime.Types;
using Annium.Serialization.Abstractions.Attributes;
using Annium.Testing;
using Annium.Testing.Collection;
using Xunit;

namespace Annium.Serialization.Json.Tests.Converters;

/// <summary>
/// Tests for constructor-based JSON converter functionality
/// </summary>
public class ConstructorJsonConverterTest : TestBase
{
    /// <summary>
    /// Tests that basic constructor-based serialization works correctly
    /// </summary>
    [Fact]
    public void Serialization_Base_Works()
    {
        // arrange
        var serializer = GetSerializer();

        var x = new A(5);

        // act
        var result = serializer.Serialize(x);

        // assert
        result.Is(@"{""value"":5,""isOdd"":false}");
    }

    /// <summary>
    /// Tests that serialization of nested constructor-based objects works correctly
    /// </summary>
    [Fact]
    public void Serialization_Inner_Works()
    {
        // arrange
        var serializer = GetSerializer();

        var x = new B(new A(2), new A(3));

        // act
        var result = serializer.Serialize(x);

        // assert
        result.Is(@"{""one"":{""value"":2,""isOdd"":true},""two"":{""value"":3,""isOdd"":false}}");
    }

    /// <summary>
    /// Tests that serialization of collections with constructor-based objects works correctly
    /// </summary>
    [Fact]
    public void Serialization_Collection_Works()
    {
        // arrange
        var serializer = GetSerializer();

        var x = new IX[] { new B(new A(2), new A(3)), new A(1) };

        // act
        var result = serializer.Serialize(x);

        // assert
        result.Is(
            @"[{""one"":{""value"":2,""isOdd"":true},""two"":{""value"":3,""isOdd"":false}},{""value"":1,""isOdd"":false}]"
        );
    }

    /// <summary>
    /// Tests that basic constructor-based deserialization works correctly
    /// </summary>
    [Fact]
    public void Deserialization_Base_Works()
    {
        // arrange
        var serializer = GetSerializer();
        var x = new A(5);
        var str = serializer.Serialize(x);

        // act
        var result = serializer.Deserialize<IX>(str);

        // assert
        result.IsEqual(x);
    }

    /// <summary>
    /// Tests that deserialization of nested constructor-based objects works correctly
    /// </summary>
    [Fact]
    public void Deserialization_Inner_Works()
    {
        // arrange
        var serializer = GetSerializer();
        var x = new B(new A(2), new A(3));
        var str = serializer.Serialize(x);

        // act
        var result = serializer.Deserialize<IX>(str);

        // assert
        result.IsEqual(x);
    }

    /// <summary>
    /// Tests that deserialization with narrow property mapping works correctly
    /// </summary>
    [Fact]
    public void Deserialization_Narrow_Works()
    {
        // arrange
        var serializer = GetSerializer();
        var x = new A(5);
        var str = serializer.Serialize(x);

        // act
        var result = serializer.Deserialize<Narrow>(str);

        // assert
        result.IsEqual(x);
    }

    /// <summary>
    /// Tests that deserialization with extra properties works correctly
    /// </summary>
    [Fact]
    public void Deserialization_Extra_Works()
    {
        // arrange
        var serializer = GetSerializer();
        var x = new Extra(5);
        var str = serializer.Serialize(x);

        // act
        var result = serializer.Deserialize<Extra>(str);

        // assert
        result.IsEqual(x);
    }

    /// <summary>
    /// Tests that deserialization with selected constructor works correctly
    /// </summary>
    [Fact]
    public void Deserialization_SelectConstructor_Works()
    {
        // arrange
        var serializer = GetSerializer();
        var x = new SelectConstructor(5, 1);
        var str = serializer.Serialize(x);

        // act
        var result = serializer.Deserialize<SelectConstructor>(str);

        // assert
        result.IsEqual(x);
    }

    /// <summary>
    /// Tests that deserialization with forced default constructor works correctly
    /// </summary>
    [Fact]
    public void Deserialization_ForceDefault_Works()
    {
        // arrange
        var serializer = GetSerializer();
        var x = new ForceDefault(5);
        var str = serializer.Serialize(x);

        // act
        var result = serializer.Deserialize<ForceDefault>(str);

        // assert
        result.IsEqual(x);
    }

    /// <summary>
    /// Tests that deserialization of collections with constructor-based objects works correctly
    /// </summary>
    [Fact]
    public void Deserialization_Collection_Works()
    {
        // arrange
        var serializer = GetSerializer();
        var x = new IX[] { new B(new A(2), new A(3)), new A(1) };
        var str = serializer.Serialize(x);

        // act
        var result = serializer.Deserialize<IReadOnlyCollection<IX>>(str);

        // assert
        result.IsEqual(x);
    }

    /// <summary>
    /// Tests that deserialization of record types with constructors works correctly
    /// </summary>
    [Fact]
    public void Deserialization_Record_Works()
    {
        // arrange
        var serializer = GetSerializer();
        var x = new MessageBase[] { new ErrorMessage("error"), new InfoMessage("info") };
        var str = serializer.Serialize(x);

        // act
        var result = serializer.Deserialize<IReadOnlyCollection<MessageBase>>(str);

        // assert
        result.Has(2);
        result.At(0).As<ErrorMessage>().Message.Is("error");
        result.At(1).As<InfoMessage>().Message.Is("info");
    }

    /// <summary>
    /// Tests that deserialization of tuple types works correctly
    /// </summary>
    [Fact]
    public void Deserialization_Tuple_Works()
    {
        // arrange
        var serializer = GetSerializer();
        var x = new[] { ("a", 1), ("b", 2) };
        var str = serializer.Serialize(x);

        // act
        var result = serializer.Deserialize<IReadOnlyCollection<ValueTuple<string, int>>>(str);

        // assert
        result.Has(2);
        result.At(0).As<ValueTuple<string, int>>().IsEqual(("a", 1));
        result.At(1).As<ValueTuple<string, int>>().IsEqual(("b", 2));
    }

    /// <summary>
    /// Test class A with constructor-based initialization
    /// </summary>
    public class A : IX
    {
        /// <summary>
        /// Gets the test value
        /// </summary>
        public int Value { get; }

        /// <summary>
        /// Gets a value indicating whether the value is odd (actually checks if even)
        /// </summary>
        public bool IsOdd => Value % 2 == 0;

        /// <summary>
        /// Initializes a new instance of the A class
        /// </summary>
        /// <param name="value">The value to initialize with</param>
        public A(int value)
        {
            Value = value;
        }
    }

    /// <summary>
    /// Test class B containing two A instances
    /// </summary>
    public class B : IX
    {
        /// <summary>
        /// Gets the first A instance
        /// </summary>
        public A One { get; }

        /// <summary>
        /// Gets the second A instance
        /// </summary>
        public A Two { get; }

        /// <summary>
        /// Initializes a new instance of the B class
        /// </summary>
        /// <param name="one">The first A instance</param>
        /// <param name="two">The second A instance</param>
        public B(A one, A two)
        {
            One = one;
            Two = two;
        }
    }

    /// <summary>
    /// Test class with narrow property mapping for deserialization testing
    /// </summary>
    public class Narrow
    {
        /// <summary>
        /// Gets the test value
        /// </summary>
        public int Value { get; }

        /// <summary>
        /// Initializes a new instance of the Narrow class
        /// </summary>
        /// <param name="value">The value to initialize with</param>
        public Narrow(int value)
        {
            Value = value;
        }
    }

    /// <summary>
    /// Test class with extra properties for deserialization testing
    /// </summary>
    public class Extra
    {
        /// <summary>
        /// Gets or sets the unique identifier
        /// </summary>
        public Guid Id { get; private set; } = Guid.NewGuid();

        /// <summary>
        /// Gets the test value
        /// </summary>
        public int Value { get; }

        /// <summary>
        /// Initializes a new instance of the Extra class
        /// </summary>
        /// <param name="value">The value to initialize with</param>
        public Extra(int value)
        {
            Value = value;
        }
    }

    /// <summary>
    /// Test class with multiple constructors to test constructor selection
    /// </summary>
    public class SelectConstructor
    {
        /// <summary>
        /// Gets or sets the unique identifier
        /// </summary>
        public Guid Id { get; private set; } = Guid.NewGuid();

        /// <summary>
        /// Gets the test value
        /// </summary>
        public int Value { get; }

        /// <summary>
        /// Initializes a new instance of the SelectConstructor class for deserialization
        /// </summary>
        /// <param name="value">The value to initialize with</param>
        [DeserializationConstructor]
        public SelectConstructor(int value)
        {
            Value = value;
        }

        /// <summary>
        /// Initializes a new instance of the SelectConstructor class with addition
        /// </summary>
        /// <param name="value">The base value</param>
        /// <param name="addition">The value to add (default 1)</param>
        public SelectConstructor(int value, int addition = 1)
        {
            Value = value + addition;
        }
    }

    /// <summary>
    /// Test class that forces use of default constructor for deserialization
    /// </summary>
    public class ForceDefault
    {
        /// <summary>
        /// Gets or sets the unique identifier
        /// </summary>
        public Guid Id { get; private set; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets the test value
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// Initializes a new instance of the ForceDefault class with value modification
        /// </summary>
        /// <param name="value">The base value (will be incremented by 1)</param>
        public ForceDefault(int value)
        {
            Value = value + 1;
        }

        /// <summary>
        /// Initializes a new instance of the ForceDefault class using default constructor for deserialization
        /// </summary>
        [DeserializationConstructor]
        private ForceDefault() { }
    }

    /// <summary>
    /// Interface for test types
    /// </summary>
    public interface IX;

    /// <summary>
    /// Test record for info messages
    /// </summary>
    /// <param name="Message">The message content</param>
    public record InfoMessage(string Message) : MessageBase;

    /// <summary>
    /// Test record for error messages
    /// </summary>
    /// <param name="Message">The message content</param>
    public record ErrorMessage(string Message) : MessageBase;

    /// <summary>
    /// Base record for message types with type resolution
    /// </summary>
    public abstract record MessageBase
    {
        /// <summary>
        /// Gets the type identifier for resolution
        /// </summary>
        [ResolutionId]
        public string Tid => GetType().GetIdString();
    }
}
