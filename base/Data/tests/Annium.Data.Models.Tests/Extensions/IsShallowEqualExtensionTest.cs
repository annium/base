using System;
using System.Collections.Generic;
using Annium.Data.Models.Extensions;
using Annium.Testing;
using Xunit;

namespace Annium.Data.Models.Tests.Extensions;

/// <summary>
/// Tests for the IsShallowEqual extension method functionality.
/// </summary>
public class IsShallowEqualExtensionTest
{
    /// <summary>
    /// Tests that shallow equality works correctly for complex objects with nested properties.
    /// </summary>
    [Fact]
    public void IsShallowEqual_Complex_Works()
    {
        // arrange
        var now = DateTimeOffset.Now;
        var demo1 = new Demo
        {
            Nullable = true,
            Uris = new[] { new Uri("http://localhost") },
            Samples = new List<Sample>
            {
                new() { Date = now, Point = new Point(1, 2) },
            },
            Points = [new Point(1, 2)],
            Dictionary = new Dictionary<Point, Sample>
            {
                {
                    new Point(1, 2),
                    new Sample { Date = now, Point = new Point(2, 1) }
                },
            },
            ReadOnlyDictionary = new Dictionary<Point, Sample>
            {
                {
                    new Point(1, 2),
                    new Sample { Date = now, Point = new Point(2, 1) }
                },
            },
        };
        var demo2 = new Demo
        {
            Nullable = true,
            Uris = new[] { new Uri("http://localhost") },
            Samples = new List<Sample>
            {
                new() { Date = now, Point = new Point(1, 2) },
            },
            Points = [.. new[] { new Point(1, 2) }],
            Dictionary = new Dictionary<Point, Sample>
            {
                {
                    new Point(1, 2),
                    new Sample { Date = now, Point = new Point(2, 1) }
                },
            },
            ReadOnlyDictionary = new Dictionary<Point, Sample>
            {
                {
                    new Point(1, 2),
                    new Sample { Date = now, Point = new Point(2, 1) }
                },
            },
        };

        // assert
        demo1.IsShallowEqual(demo2).IsTrue();
    }

    /// <summary>
    /// Tests that shallow equality works correctly for objects with properties.
    /// </summary>
    [Fact]
    public void IsShallowEqual_Property_Works()
    {
        // arrange
        var now = DateTimeOffset.Now;
        var a = new Sample { Date = now, Point = new Point(2, 1) };
        var b = new Sample { Date = now, Point = new Point(2, 1) };
        var c = new Sample { Date = now + TimeSpan.FromSeconds(1), Point = new Point(2, 1) };
        Sample d = default;
        var e = a;

        // assert
        a.IsShallowEqual(b).IsTrue();
        a.IsShallowEqual(c).IsFalse();
        a.IsShallowEqual(d).IsFalse();
        a.IsShallowEqual(e).IsTrue();
    }

    /// <summary>
    /// Tests that shallow equality works correctly for arrays.
    /// </summary>
    [Fact]
    public void IsShallowEqual_Array_Works()
    {
        // arrange
        var now = DateTimeOffset.Now;
        Sample[] a =
        {
            new() { Date = now, Point = new Point(2, 1) },
        };
        Sample[] b =
        {
            new() { Date = now, Point = new Point(2, 1) },
        };
        Sample[] c =
        {
            new() { Date = now + TimeSpan.FromSeconds(1), Point = new Point(2, 1) },
        };
        Sample[] d = { default };
        var e = a;

        // assert
        a.IsShallowEqual(b).IsTrue();
        a.IsShallowEqual(c).IsFalse();
        a.IsShallowEqual(d).IsFalse();
        a.IsShallowEqual(e).IsTrue();
    }

    /// <summary>
    /// Tests that shallow equality works correctly for lists.
    /// </summary>
    [Fact]
    public void IsShallowEqual_List_Works()
    {
        // arrange
        var now = DateTimeOffset.Now;
        IList<Sample> a = new List<Sample>
        {
            new() { Date = now, Point = new Point(2, 1) },
        };
        IList<Sample> b = new List<Sample>
        {
            new() { Date = now, Point = new Point(2, 1) },
        };
        IList<Sample> c = new List<Sample>
        {
            new() { Date = now + TimeSpan.FromSeconds(1), Point = new Point(2, 1) },
        };
        IList<Sample> d = new List<Sample> { default };
        var e = a;

        // assert
        a.IsShallowEqual(b).IsTrue();
        a.IsShallowEqual(c).IsFalse();
        a.IsShallowEqual(d).IsFalse();
        a.IsShallowEqual(e).IsTrue();
    }

    /// <summary>
    /// Tests that shallow equality works correctly for IDictionary instances.
    /// </summary>
    [Fact]
    public void IsShallowEqual_IDictionary_Works()
    {
        // arrange
        var now = DateTimeOffset.Now;
        IDictionary<Key, Sample> a = new Dictionary<Key, Sample>
        {
            {
                new Key(1, 1),
                new Sample { Date = now, Point = new Point(2, 1) }
            },
        };
        IDictionary<Key, Sample> b = new Dictionary<Key, Sample>
        {
            {
                new Key(1, 1),
                new Sample { Date = now, Point = new Point(2, 1) }
            },
        };
        IDictionary<Key, Sample> c = new Dictionary<Key, Sample>
        {
            {
                new Key(1, 2),
                new Sample { Date = now, Point = new Point(2, 1) }
            },
        };
        IDictionary<Key, Sample> d = new Dictionary<Key, Sample> { { new Key(0, 0), default } };
        IDictionary<Key, Sample> e = a;

        // assert
        a.IsShallowEqual(b).IsTrue();
        a.IsShallowEqual(c).IsFalse();
        a.IsShallowEqual(d).IsFalse();
        a.IsShallowEqual(e).IsTrue();
    }

    /// <summary>
    /// Tests that shallow equality works correctly for IReadOnlyDictionary instances.
    /// </summary>
    [Fact]
    public void IsShallowEqual_IReadOnlyDictionary_Works()
    {
        // arrange
        var now = DateTimeOffset.Now;
        IReadOnlyDictionary<Key, Sample> a = new Dictionary<Key, Sample>
        {
            {
                new Key(1, 1),
                new Sample { Date = now, Point = new Point(2, 1) }
            },
        };
        IReadOnlyDictionary<Key, Sample> b = new Dictionary<Key, Sample>
        {
            {
                new Key(1, 1),
                new Sample { Date = now, Point = new Point(2, 1) }
            },
        };
        IReadOnlyDictionary<Key, Sample> c = new Dictionary<Key, Sample>
        {
            {
                new Key(1, 2),
                new Sample { Date = now, Point = new Point(2, 1) }
            },
        };
        IReadOnlyDictionary<Key, Sample> d = new Dictionary<Key, Sample> { { new Key(0, 0), default } };
        IReadOnlyDictionary<Key, Sample> e = a;

        // assert
        a.IsShallowEqual(b).IsTrue();
        a.IsShallowEqual(c).IsFalse();
        a.IsShallowEqual(d).IsFalse();
        a.IsShallowEqual(e).IsTrue();
    }

    /// <summary>
    /// Tests that shallow equality works correctly for objects with recursive references.
    /// </summary>
    [Fact]
    public void IsShallowEqual_Recursive_Works()
    {
        // arrange
        var a = new Exception("x", new Exception("y"));
        var b = new Exception("x", new Exception("y"));

        // assert
        a.IsShallowEqual(b).IsTrue();
    }

    /// <summary>
    /// Tests that shallow equality works correctly when comparing with null values.
    /// </summary>
    [Fact]
    public void IsShallowEqual_ToNull_Works()
    {
        // arrange
        Exception a = new("x");
        Exception b = default!;

        // assert
        a.IsShallowEqual(b).IsFalse();
        b.IsShallowEqual(a).IsFalse();
    }

    /// <summary>
    /// Tests that shallow equality works correctly when comparing with anonymous objects.
    /// </summary>
    [Fact]
    // TODO: fix, not valid
    public void IsShallowEqual_ToAnonymousObject_Works()
    {
        // arrange
        var now = DateTimeOffset.Now;
        var src = new Big
        {
            Samples = new[]
            {
                new Sample { Date = now + TimeSpan.FromDays(1), Point = new Point(1, 2) },
                new Sample { Date = now + TimeSpan.FromHours(1), Point = new Point(4, 3) },
            },
            Keys = new Dictionary<string, Key> { { "a", new Key(5, 3) }, { "c", new Key(2, 4) } },
        };
        var tgt = new
        {
            Samples = new[]
            {
                new { Date = now + TimeSpan.FromDays(1), Point = new Point(1, 2) },
                new { Date = now + TimeSpan.FromHours(1), Point = new Point(4, 3) },
            },
            Keys = new Dictionary<string, Key> { { "a", new Key(5, 3) }, { "c", new Key(2, 4) } },
        };

        // assert
        src.IsShallowEqual(tgt).IsFalse();
    }
}

/// <summary>
/// Test class representing a complex demo object with various property types.
/// </summary>
internal class Demo
{
    /// <summary>
    /// Gets or sets a nullable boolean value.
    /// </summary>
    public bool? Nullable { get; set; }

    /// <summary>
    /// Gets or sets a collection of URI objects.
    /// </summary>
    public IEnumerable<Uri> Uris { get; set; } = Array.Empty<Uri>();

    /// <summary>
    /// Gets or sets a list of sample objects.
    /// </summary>
    public List<Sample> Samples { get; set; } = new();

    /// <summary>
    /// Gets or sets a hash set of point objects.
    /// </summary>
    public HashSet<Point> Points { get; set; } = new();

    /// <summary>
    /// Gets or sets a dictionary mapping points to samples.
    /// </summary>
    public IDictionary<Point, Sample> Dictionary { get; set; } = new Dictionary<Point, Sample>();

    /// <summary>
    /// Gets or sets a read-only dictionary mapping points to samples.
    /// </summary>
    public IReadOnlyDictionary<Point, Sample> ReadOnlyDictionary { get; set; } = new Dictionary<Point, Sample>();
}

/// <summary>
/// Test class representing a large object with collections.
/// </summary>
internal class Big
{
    /// <summary>
    /// Gets or sets a collection of sample objects.
    /// </summary>
    public IEnumerable<Sample> Samples { get; set; } = Array.Empty<Sample>();

    /// <summary>
    /// Gets or sets a dictionary mapping strings to key objects.
    /// </summary>
    public Dictionary<string, Key> Keys { get; set; } = new();
}

/// <summary>
/// Test struct representing a sample with date and point data.
/// </summary>
internal struct Sample
{
    /// <summary>
    /// Gets or sets the date and time value.
    /// </summary>
    public DateTimeOffset Date { get; set; }

    /// <summary>
    /// Gets or sets the point coordinates.
    /// </summary>
    public Point Point { get; set; }
}

/// <summary>
/// Test class representing a key with X and Y coordinates.
/// </summary>
internal class Key
{
    /// <summary>
    /// Gets the X coordinate value.
    /// </summary>
    public int X { get; }

    /// <summary>
    /// Gets the Y coordinate value.
    /// </summary>
    public int Y { get; }

    /// <summary>
    /// Initializes a new instance of the Key class.
    /// </summary>
    /// <param name="x">The X coordinate value.</param>
    /// <param name="y">The Y coordinate value.</param>
    public Key(int x, int y)
    {
        X = x;
        Y = y;
    }
}
