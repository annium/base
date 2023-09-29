using System;
using System.Collections.Generic;
using Annium.Core.Mapper.Attributes;
using Annium.Core.Runtime.Types;

namespace Annium.Configuration.Tests.Lib;

public sealed record Config
{
    public bool Flag { get; set; }
    public int Plain { get; set; }
    public decimal? Nullable { get; set; }
    public int[] Array { get; set; } = System.Array.Empty<int>();
    public List<int[]> Matrix { get; set; } = new();
    public List<Val> List { get; set; } = new();
    public Dictionary<string, Val> Dictionary { get; set; } = new();
    public Val Nested { get; set; } = new();

    public SomeConfig Abstract { get; set; } = new ConfigOne();
    public SomeEnum Enum { get; set; }
    public ValueTuple<string, int> Tuple { get; set; }
}

public sealed record Val
{
    public int Plain { get; set; }

    public decimal[] Array { get; set; } = System.Array.Empty<decimal>();
}

public abstract record SomeConfig
{
    [ResolutionKey]
    public string Type { get; protected set; } = string.Empty;
}

[ResolutionKeyValue(nameof(ConfigOne))]
public sealed record ConfigOne : SomeConfig
{
    public uint Value { get; set; }

    public ConfigOne()
    {
        Type = nameof(ConfigOne);
    }
}

[ResolutionKeyValue(nameof(ConfigTwo))]
public sealed record ConfigTwo : SomeConfig
{
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
    Two
}