using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Annium.Net.Types.Refs;
using NodaTime;

namespace Annium.Net.Types.Internal.Config;

/// <summary>
/// Extension methods for configuring mapper with default type mappings and rules
/// </summary>
internal static class MapperConfigExtensions
{
    /// <summary>
    /// Configures the mapper with standard base type mappings for primitive and common .NET types
    /// </summary>
    /// <param name="cfg">The mapper configuration to extend</param>
    /// <returns>The mapper configuration for method chaining</returns>
    public static IMapperConfig SetBaseTypes(this IMapperConfig cfg) =>
        cfg.SetBaseType(typeof(object), BaseType.Object)
            .SetBaseType(typeof(bool), BaseType.Bool)
            .SetBaseType(typeof(string), BaseType.String)
            .SetBaseType(typeof(byte), BaseType.Byte)
            .SetBaseType(typeof(sbyte), BaseType.SByte)
            .SetBaseType(typeof(int), BaseType.Int)
            .SetBaseType(typeof(uint), BaseType.UInt)
            .SetBaseType(typeof(long), BaseType.Long)
            .SetBaseType(typeof(ulong), BaseType.ULong)
            .SetBaseType(typeof(float), BaseType.Float)
            .SetBaseType(typeof(double), BaseType.Double)
            .SetBaseType(typeof(decimal), BaseType.Decimal)
            .SetBaseType(typeof(Guid), BaseType.Guid)
            .SetBaseType(typeof(DateTime), BaseType.DateTime)
            .SetBaseType(typeof(DateTimeOffset), BaseType.DateTimeOffset)
            .SetBaseType(typeof(DateOnly), BaseType.Date)
            .SetBaseType(typeof(TimeOnly), BaseType.Time)
            .SetBaseType(typeof(TimeSpan), BaseType.TimeSpan)
            .SetBaseType(typeof(Instant), BaseType.DateTimeOffset)
            .SetBaseType(typeof(LocalDate), BaseType.Date)
            .SetBaseType(typeof(LocalTime), BaseType.Time)
            .SetBaseType(typeof(LocalDateTime), BaseType.DateTime)
            .SetBaseType(typeof(ZonedDateTime), BaseType.DateTimeOffset)
            .SetBaseType(typeof(Duration), BaseType.TimeSpan)
            .SetBaseType(typeof(Period), BaseType.TimeSpan)
            .SetBaseType(typeof(YearMonth), BaseType.YearMonth)
            .SetBaseType(typeof(void), BaseType.Void);

    /// <summary>
    /// Configures the mapper to ignore common system types and interfaces that should not be mapped
    /// </summary>
    /// <param name="cfg">The mapper configuration to extend</param>
    /// <returns>The mapper configuration for method chaining</returns>
    public static IMapperConfig Ignore(this IMapperConfig cfg) =>
        cfg
        // basic types
        .Ignore(Match.Is(typeof(object)))
            .Ignore(Match.Is(typeof(ValueType)))
            // enumerable interfaces
            .Ignore(Match.Is(typeof(IEnumerable<>)))
            .Ignore(Match.Is(typeof(ICollection<>)))
            .Ignore(Match.Is(typeof(IReadOnlyCollection<>)))
            // dictionary interfaces
            .Ignore(Match.Is(typeof(IReadOnlyDictionary<,>)))
            .Ignore(Match.Is(typeof(IDictionary<,>)))
            // base type interfaces
            .Ignore(Match.Is(typeof(IComparable<>)))
            .Ignore(Match.Is(typeof(IEquatable<>)))
            // low-level interfaces
            .Ignore(Match.Is(typeof(ISpanParsable<>)))
            .Ignore(Match.Is(typeof(IParsable<>)))
            // tasks
            .Ignore(Match.IsDerivedFrom(typeof(Task), self: true))
            .Ignore(Match.IsDerivedFrom(typeof(Task<>), self: true))
            .Ignore(Match.Is(typeof(ValueTask)))
            .Ignore(Match.Is(typeof(ValueTask<>)))
            // custom basic interfaces
            .Ignore(Match.Is(typeof(ICopyable<>)));

    /// <summary>
    /// Configures the mapper to recognize standard collection interfaces and types as arrays for mapping
    /// </summary>
    /// <param name="cfg">The mapper configuration to extend</param>
    /// <returns>The mapper configuration for method chaining</returns>
    public static IMapperConfig RegisterArrays(this IMapperConfig cfg) =>
        cfg.SetArray(typeof(IEnumerable<>))
            .SetArray(typeof(IReadOnlyCollection<>))
            .SetArray(typeof(ICollection<>))
            .SetArray(typeof(IReadOnlyList<>))
            .SetArray(typeof(IList<>))
            .SetArray(typeof(IReadOnlySet<>))
            .SetArray(typeof(ISet<>))
            .SetArray(typeof(List<>))
            .SetArray(typeof(HashSet<>));

    /// <summary>
    /// Configures the mapper to recognize standard dictionary interfaces and types as records for mapping
    /// </summary>
    /// <param name="cfg">The mapper configuration to extend</param>
    /// <returns>The mapper configuration for method chaining</returns>
    public static IMapperConfig RegisterRecords(this IMapperConfig cfg) =>
        cfg.SetRecord(typeof(IDictionary<,>))
            .SetRecord(typeof(IReadOnlyDictionary<,>))
            .SetRecord(typeof(Dictionary<,>))
            .SetRecord(typeof(ImmutableDictionary<,>));
}
