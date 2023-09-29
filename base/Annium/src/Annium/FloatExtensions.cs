using System;
using System.Runtime.CompilerServices;

namespace Annium;

public static class FloatExtensions
{
    public static float DiffFrom(this float value, float from) =>
        from == 0f ? value == 0f ? 0 : float.PositiveInfinity : Math.Abs((value - from) / from);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAround(this float value, float to, float precision) =>
        value.DiffFrom(to) <= precision;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int FloorInt32(this float value) =>
        (int)Math.Floor(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long FloorInt64(this float value) =>
        (long)Math.Floor(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Floor(this float value) =>
        (float)Math.Floor(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int RoundInt32(this float value) =>
        (int)Math.Round(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int RoundInt32(this float value, MidpointRounding mode) =>
        (int)Math.Round(value, mode);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long RoundInt64(this float value) =>
        (long)Math.Round(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long RoundInt64(this float value, MidpointRounding mode) =>
        (long)Math.Round(value, mode);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Round(this float value) =>
        (float)Math.Round(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Round(this float value, int digits) =>
        (float)Math.Round(value, digits);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Round(this float value, MidpointRounding mode) =>
        (float)Math.Round(value, mode);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Round(this float value, int digits, MidpointRounding mode) =>
        (float)Math.Round(value, digits, mode);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CeilInt32(this float value) =>
        (int)Math.Ceiling(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long CeilInt64(this float value) =>
        (long)Math.Ceiling(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Ceil(this float value) =>
        (float)Math.Ceiling(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Within(this float value, float min, float max) => value.Above(min).Below(max);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Above(this float value, float min) => Math.Max(value, min);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Below(this float value, float max) => Math.Min(value, max);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float FloorTo(this float value, float step) => value - value % step;

    public static float RoundTo(this float value, float step)
    {
        var diff = value % step;

        return value - diff + (step > diff * 2f ? 0f : step);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float CeilTo(this float value, float step) => value + step - value % step;
}