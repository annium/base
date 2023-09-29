using System;
using System.Runtime.CompilerServices;

namespace Annium;

public static class DoubleExtensions
{
    public static double DiffFrom(this double value, double from) =>
        from == 0d ? value == 0d ? 0 : double.PositiveInfinity : Math.Abs((value - from) / from);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAround(this double value, double to, double precision) =>
        value.DiffFrom(to) <= precision;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int FloorInt32(this double value) =>
        (int)Math.Floor(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long FloorInt64(this double value) =>
        (long)Math.Floor(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Floor(this double value) =>
        Math.Floor(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int RoundInt32(this double value) =>
        (int)Math.Round(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int RoundInt32(this double value, MidpointRounding mode) =>
        (int)Math.Round(value, mode);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long RoundInt64(this double value) =>
        (long)Math.Round(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long RoundInt64(this double value, MidpointRounding mode) =>
        (long)Math.Round(value, mode);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Round(this double value) =>
        Math.Round(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Round(this double value, int digits) =>
        Math.Round(value, digits);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Round(this double value, MidpointRounding mode) =>
        Math.Round(value, mode);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Round(this double value, int digits, MidpointRounding mode) =>
        Math.Round(value, digits, mode);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CeilInt32(this double value) =>
        (int)Math.Ceiling(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long CeilInt64(this double value) =>
        (long)Math.Ceiling(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Ceil(this double value) =>
        Math.Ceiling(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Within(this double value, double min, double max) => value.Above(min).Below(max);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Above(this double value, double min) => Math.Max(value, min);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Below(this double value, double max) => Math.Min(value, max);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double FloorTo(this double value, double step) => value - value % step;

    public static double RoundTo(this double value, double step)
    {
        var diff = value % step;

        return value - diff + (step > diff * 2d ? 0d : step);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double CeilTo(this double value, double step) => value + step - value % step;
}