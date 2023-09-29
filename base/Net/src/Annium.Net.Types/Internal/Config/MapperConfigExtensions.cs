using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Annium.Net.Types.Refs;
using NodaTime;

namespace Annium.Net.Types.Internal.Config;

internal static class MapperConfigExtensions
{
    public static IMapperConfig SetBaseTypes(this IMapperConfig cfg) => cfg
        .SetBaseType(typeof(object), BaseType.Object)
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

    public static IMapperConfig Ignore(this IMapperConfig cfg) => cfg
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

    public static IMapperConfig RegisterArrays(this IMapperConfig cfg) => cfg
        .SetArray(typeof(IEnumerable<>))
        .SetArray(typeof(IReadOnlyCollection<>))
        .SetArray(typeof(ICollection<>))
        .SetArray(typeof(IReadOnlyList<>))
        .SetArray(typeof(IList<>))
        .SetArray(typeof(IReadOnlySet<>))
        .SetArray(typeof(ISet<>))
        .SetArray(typeof(List<>))
        .SetArray(typeof(HashSet<>));

    public static IMapperConfig RegisterRecords(this IMapperConfig cfg) => cfg
        .SetRecord(typeof(IDictionary<,>))
        .SetRecord(typeof(IReadOnlyDictionary<,>))
        .SetRecord(typeof(Dictionary<,>))
        .SetRecord(typeof(ImmutableDictionary<,>));
}