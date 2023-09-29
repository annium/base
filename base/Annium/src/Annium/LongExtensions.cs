using System;
using System.Runtime.CompilerServices;

namespace Annium;

public static class LongExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long Within(this long value, long min, long max) => value.Above(min).Below(max);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long Above(this long value, long min) => Math.Max(value, min);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long Below(this long value, long max) => Math.Min(value, max);
}