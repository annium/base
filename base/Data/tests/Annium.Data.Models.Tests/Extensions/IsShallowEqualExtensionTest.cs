using System;
using System.Collections.Generic;
using Annium.Data.Models.Extensions;
using Annium.Testing;
using Xunit;

namespace Annium.Data.Models.Tests.Extensions;

public class IsShallowEqualExtensionTest
{
    [Fact]
    public void IsShallowEqual_Complex_Works()
    {
        // arrange
        var now = DateTimeOffset.Now;
        var demo1 = new Demo
        {
            Nullable = true,
            Uris = new[] { new Uri("http://localhost") },
            Samples = new List<Sample> { new() { Date = now, Point = new Point(1, 2) } },
            Points = new HashSet<Point>(new[] { new Point(1, 2) }),
            Dictionary = new Dictionary<Point, Sample> { { new Point(1, 2), new Sample { Date = now, Point = new Point(2, 1) } } },
            ReadOnlyDictionary = new Dictionary<Point, Sample> { { new Point(1, 2), new Sample { Date = now, Point = new Point(2, 1) } } },
        };
        var demo2 = new Demo
        {
            Nullable = true,
            Uris = new[] { new Uri("http://localhost") },
            Samples = new List<Sample> { new() { Date = now, Point = new Point(1, 2) } },
            Points = new HashSet<Point>(new[] { new Point(1, 2) }),
            Dictionary = new Dictionary<Point, Sample> { { new Point(1, 2), new Sample { Date = now, Point = new Point(2, 1) } } },
            ReadOnlyDictionary = new Dictionary<Point, Sample> { { new Point(1, 2), new Sample { Date = now, Point = new Point(2, 1) } } },
        };

        // assert
        demo1.IsShallowEqual(demo2).IsTrue();
    }

    [Fact]
    public void IsShallowEqual_Property_Works()
    {
        // arrange
        var now = DateTimeOffset.Now;
        Sample a = new Sample { Date = now, Point = new Point(2, 1) };
        Sample b = new Sample { Date = now, Point = new Point(2, 1) };
        Sample c = new Sample { Date = now + TimeSpan.FromSeconds(1), Point = new Point(2, 1) };
        Sample d = default;
        Sample e = a;

        // assert
        a.IsShallowEqual(b).IsTrue();
        a.IsShallowEqual(c).IsFalse();
        a.IsShallowEqual(d).IsFalse();
        a.IsShallowEqual(e).IsTrue();
    }

    [Fact]
    public void IsShallowEqual_Array_Works()
    {
        // arrange
        var now = DateTimeOffset.Now;
        Sample[] a = { new() { Date = now, Point = new Point(2, 1) } };
        Sample[] b = { new() { Date = now, Point = new Point(2, 1) } };
        Sample[] c = { new() { Date = now + TimeSpan.FromSeconds(1), Point = new Point(2, 1) } };
        Sample[] d = { default };
        Sample[] e = a;

        // assert
        a.IsShallowEqual(b).IsTrue();
        a.IsShallowEqual(c).IsFalse();
        a.IsShallowEqual(d).IsFalse();
        a.IsShallowEqual(e).IsTrue();
    }

    [Fact]
    public void IsShallowEqual_List_Works()
    {
        // arrange
        var now = DateTimeOffset.Now;
        IList<Sample> a = new List<Sample> { new() { Date = now, Point = new Point(2, 1) } };
        IList<Sample> b = new List<Sample> { new() { Date = now, Point = new Point(2, 1) } };
        IList<Sample> c = new List<Sample> { new() { Date = now + TimeSpan.FromSeconds(1), Point = new Point(2, 1) } };
        IList<Sample> d = new List<Sample> { default };
        IList<Sample> e = a;

        // assert
        a.IsShallowEqual(b).IsTrue();
        a.IsShallowEqual(c).IsFalse();
        a.IsShallowEqual(d).IsFalse();
        a.IsShallowEqual(e).IsTrue();
    }

    [Fact]
    public void IsShallowEqual_IDictionary_Works()
    {
        // arrange
        var now = DateTimeOffset.Now;
        IDictionary<Key, Sample> a = new Dictionary<Key, Sample> { { new Key(1, 1), new Sample { Date = now, Point = new Point(2, 1) } } };
        IDictionary<Key, Sample> b = new Dictionary<Key, Sample> { { new Key(1, 1), new Sample { Date = now, Point = new Point(2, 1) } } };
        IDictionary<Key, Sample> c = new Dictionary<Key, Sample> { { new Key(1, 2), new Sample { Date = now, Point = new Point(2, 1) } } };
        IDictionary<Key, Sample> d = new Dictionary<Key, Sample> { { new Key(0, 0), default } };
        IDictionary<Key, Sample> e = a;

        // assert
        a.IsShallowEqual(b).IsTrue();
        a.IsShallowEqual(c).IsFalse();
        a.IsShallowEqual(d).IsFalse();
        a.IsShallowEqual(e).IsTrue();
    }

    [Fact]
    public void IsShallowEqual_IReadOnlyDictionary_Works()
    {
        // arrange
        var now = DateTimeOffset.Now;
        IReadOnlyDictionary<Key, Sample> a = new Dictionary<Key, Sample> { { new Key(1, 1), new Sample { Date = now, Point = new Point(2, 1) } } };
        IReadOnlyDictionary<Key, Sample> b = new Dictionary<Key, Sample> { { new Key(1, 1), new Sample { Date = now, Point = new Point(2, 1) } } };
        IReadOnlyDictionary<Key, Sample> c = new Dictionary<Key, Sample> { { new Key(1, 2), new Sample { Date = now, Point = new Point(2, 1) } } };
        IReadOnlyDictionary<Key, Sample> d = new Dictionary<Key, Sample> { { new Key(0, 0), default } };
        IReadOnlyDictionary<Key, Sample> e = a;

        // assert
        a.IsShallowEqual(b).IsTrue();
        a.IsShallowEqual(c).IsFalse();
        a.IsShallowEqual(d).IsFalse();
        a.IsShallowEqual(e).IsTrue();
    }

    [Fact]
    public void IsShallowEqual_Recursive_Works()
    {
        // arrange
        var a = new Exception("x", new Exception("y"));
        var b = new Exception("x", new Exception("y"));

        // assert
        a.IsShallowEqual(b).IsTrue();
    }

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
                new Sample
                {
                    Date = now + TimeSpan.FromDays(1),
                    Point = new Point(1, 2)
                },
                new Sample
                {
                    Date = now + TimeSpan.FromHours(1),
                    Point = new Point(4, 3)
                }
            },
            Keys = new Dictionary<string, Key>
            {
                { "a", new Key(5, 3) },
                { "c", new Key(2, 4) },
            }
        };
        var tgt = new
        {
            Samples = new[]
            {
                new
                {
                    Date = now + TimeSpan.FromDays(1),
                    Point = new Point(1, 2)
                },
                new
                {
                    Date = now + TimeSpan.FromHours(1),
                    Point = new Point(4, 3)
                }
            },
            Keys = new Dictionary<string, Key>
            {
                { "a", new Key(5, 3) },
                { "c", new Key(2, 4) },
            }
        };

        // assert
        src.IsShallowEqual(tgt).IsFalse();
    }
}

internal class Demo
{
    public bool? Nullable { get; set; }

    public IEnumerable<Uri> Uris { get; set; } = Array.Empty<Uri>();
    public List<Sample> Samples { get; set; } = new();

    public HashSet<Point> Points { get; set; } = new();
    public IDictionary<Point, Sample> Dictionary { get; set; } = new Dictionary<Point, Sample>();
    public IReadOnlyDictionary<Point, Sample> ReadOnlyDictionary { get; set; } = new Dictionary<Point, Sample>();
}

internal class Big
{
    public IEnumerable<Sample> Samples { get; set; } = Array.Empty<Sample>();
    public Dictionary<string, Key> Keys { get; set; } = new();
}

internal struct Sample
{
    public DateTimeOffset Date { get; set; }
    public Point Point { get; set; }
}

internal class Key
{
    public int X { get; }
    public int Y { get; }

    public Key(int x, int y)
    {
        X = x;
        Y = y;
    }
}