using System;
using System.Runtime.CompilerServices;

namespace Annium;

public static class IntExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Within(this int value, int min, int max) => value.Above(min).Below(max);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Above(this int value, int min) => Math.Max(value, min);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Below(this int value, int max) => Math.Min(value, max);
}