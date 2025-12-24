using System;
using System.Collections.Generic;
using Annium.Linq;
using Annium.Testing;
using Xunit;

namespace Annium.Tests.Linq;

/// <summary>
/// Contains unit tests for Dictionary extension methods.
/// </summary>
public class DictionaryExtensionsTest
{
    /// <summary>
    /// Verifies that RemoveAll removes entries matching the key predicate.
    /// </summary>
    [Fact]
    public void RemoveAll_ByKeyPredicate_RemovesMatchingEntries()
    {
        // arrange
        IDictionary<int, string> data = new Dictionary<int, string>
        {
            { 1, "one" },
            { 2, "two" },
            { 3, "three" },
        };

        // act
        data.RemoveAll(key => key % 2 == 0);

        // assert
        data.Has(2);
        data.ContainsKey(2).IsFalse();
        data.At(1).Is("one");
        data.At(3).Is("three");
    }

    /// <summary>
    /// Verifies that RemoveAll removes entries matching the value predicate.
    /// </summary>
    [Fact]
    public void RemoveAll_ByValuePredicate_RemovesMatchingEntries()
    {
        // arrange
        IDictionary<string, int> data = new Dictionary<string, int>
        {
            { "a", 1 },
            { "b", 2 },
            { "c", 1 },
        };

        // act
        data.RemoveAll(value => value == 1);

        // assert
        data.Has(1);
        data.ContainsKey("a").IsFalse();
        data.ContainsKey("c").IsFalse();
        data.At("b").Is(2);
    }

    /// <summary>
    /// Verifies that RemoveAll removes entries matching the key-value predicate.
    /// </summary>
    [Fact]
    public void RemoveAll_ByPairPredicate_RemovesMatchingEntries()
    {
        // arrange
        IDictionary<string, int> data = new Dictionary<string, int>
        {
            { "keep", 1 },
            { "drop", 4 },
            { "hold", 3 },
        };

        // act
        data.RemoveAll((key, value) => key.Length == value);

        // assert
        data.Has(2);
        data.ContainsKey("drop").IsFalse();
        data.At("keep").Is(1);
        data.At("hold").Is(3);
    }

    /// <summary>
    /// Verifies that Merge with key-value pairs respects merge behavior.
    /// </summary>
    [Fact]
    public void Merge_KeyValuePairs_RespectsBehavior()
    {
        // arrange
        var target = new[] { new KeyValuePair<int, string>(2, "override"), new KeyValuePair<int, string>(3, "three") };

        // act
        IDictionary<int, string> keepTarget = new Dictionary<int, string> { { 1, "one" }, { 2, "two" } }.Merge(
            target,
            MergeBehavior.KeepTarget
        );
        IDictionary<int, string> keepSource = new Dictionary<int, string> { { 1, "one" }, { 2, "two" } }.Merge(
            target,
            MergeBehavior.KeepSource
        );

        // assert
        keepTarget.Has(3);
        keepTarget.At(1).Is("one");
        keepTarget.At(2).Is("override");
        keepTarget.At(3).Is("three");

        keepSource.Has(3);
        keepSource.At(1).Is("one");
        keepSource.At(2).Is("two");
        keepSource.At(3).Is("three");
    }

    /// <summary>
    /// Verifies that Merge with value selector respects merge behavior.
    /// </summary>
    [Fact]
    public void Merge_ValueSelector_RespectsBehavior()
    {
        // arrange
        var target = new[] { new Item(2, "override"), new Item(3, "three") };

        // act
        IDictionary<int, Item> keepTarget = new Dictionary<int, Item>
        {
            { 1, new(1, "one") },
            { 2, new(2, "two") },
        }.Merge(target, x => x.Id, MergeBehavior.KeepTarget);
        IDictionary<int, Item> keepSource = new Dictionary<int, Item>
        {
            { 1, new(1, "one") },
            { 2, new(2, "two") },
        }.Merge(target, x => x.Id, MergeBehavior.KeepSource);

        // assert
        keepTarget.Has(3);
        keepTarget.At(1).Name.Is("one");
        keepTarget.At(2).Name.Is("override");
        keepTarget.At(3).Name.Is("three");

        keepSource.Has(3);
        keepSource.At(1).Name.Is("one");
        keepSource.At(2).Name.Is("two");
        keepSource.At(3).Name.Is("three");
    }

    /// <summary>
    /// Verifies that MapValue returns value for existing keys and throws otherwise.
    /// </summary>
    [Fact]
    public void MapValue_ReturnsValueOrThrows()
    {
        // arrange
        var dictionary = new Dictionary<string, int> { { "key", 42 } };

        // act & assert
        dictionary.MapValue("key").Is(42);
        Wrap.It(() => dictionary.MapValue("missing")).Throws<ArgumentOutOfRangeException>();
        Wrap.It(() => dictionary.MapValue(null)).Throws<ArgumentOutOfRangeException>();
    }

    /// <summary>
    /// Verifies that TryMapValue returns value for existing keys or default otherwise.
    /// </summary>
    [Fact]
    public void TryMapValue_ReturnsValueOrDefault()
    {
        // arrange
        var dictionary = new Dictionary<string, string> { { "key", "value" } };

        // act & assert
        dictionary.TryMapValue("key").Is("value");
        dictionary.TryMapValue("missing").IsDefault();
        dictionary.TryMapValue(null).IsDefault();
    }

    private record Item(int Id, string Name);
}
