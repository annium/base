using System;
using System.Collections.Generic;
using Annium.Core.Mapper.Attributes;
using Annium.Core.Runtime.Types;

namespace Annium.Configuration.Tests.Lib;

/// <summary>
/// Configuration model for testing various data types and structures.
/// </summary>
public sealed record Config
{
    /// <summary>
    /// Gets or sets a boolean flag value.
    /// </summary>
    public bool Flag { get; set; }

    /// <summary>
    /// Gets or sets a plain integer value.
    /// </summary>
    public int Plain { get; set; }

    /// <summary>
    /// Gets or sets a nullable decimal value.
    /// </summary>
    public decimal? Nullable { get; set; }

    /// <summary>
    /// Gets or sets an array of integers.
    /// </summary>
    public int[] Array { get; set; } = System.Array.Empty<int>();

    /// <summary>
    /// Gets or sets a matrix (list of integer arrays).
    /// </summary>
    public List<int[]> Matrix { get; set; } = new();

    /// <summary>
    /// Gets or sets a list of Val objects.
    /// </summary>
    public List<Val> List { get; set; } = new();

    /// <summary>
    /// Gets or sets a dictionary of string keys to Val values.
    /// </summary>
    public Dictionary<string, Val> Dictionary { get; set; } = new();

    /// <summary>
    /// Gets or sets a nested Val object.
    /// </summary>
    public Val Nested { get; set; } = new();

    /// <summary>
    /// Gets or sets an abstract configuration object.
    /// </summary>
    public SomeConfig Abstract { get; set; } = new ConfigOne();

    /// <summary>
    /// Gets or sets an enum value.
    /// </summary>
    public SomeEnum Enum { get; set; }

    /// <summary>
    /// Gets or sets a tuple of string and integer.
    /// </summary>
    public ValueTuple<string, int> Tuple { get; set; }
}

/// <summary>
/// Value object for testing nested configuration structures.
/// </summary>
public sealed record Val
{
    /// <summary>
    /// Gets or sets a plain integer value.
    /// </summary>
    public int Plain { get; set; }

    /// <summary>
    /// Gets or sets an array of decimal values.
    /// </summary>
    public decimal[] Array { get; set; } = System.Array.Empty<decimal>();
}

/// <summary>
/// Abstract base configuration class for testing polymorphic configuration.
/// </summary>
public abstract record SomeConfig
{
    /// <summary>
    /// Gets or sets the type identifier for resolution.
    /// </summary>
    [ResolutionKey]
    public string Type { get; protected set; } = string.Empty;
}

/// <summary>
/// First concrete configuration type for testing polymorphic resolution.
/// </summary>
[ResolutionKeyValue(nameof(ConfigOne))]
public sealed record ConfigOne : SomeConfig
{
    /// <summary>
    /// Gets or sets an unsigned integer value.
    /// </summary>
    public uint Value { get; set; }

    public ConfigOne()
    {
        Type = nameof(ConfigOne);
    }
}

/// <summary>
/// Second concrete configuration type for testing polymorphic resolution.
/// </summary>
[ResolutionKeyValue(nameof(ConfigTwo))]
public sealed record ConfigTwo : SomeConfig
{
    /// <summary>
    /// Gets or sets a long integer value.
    /// </summary>
    public long Value { get; set; }

    public ConfigTwo()
    {
        Type = nameof(ConfigTwo);
    }
}

[AutoMapped]
public enum SomeEnum
{
    One,
    Two,
}
