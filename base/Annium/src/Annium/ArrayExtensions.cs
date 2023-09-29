using System.Linq;
using System.Runtime.CompilerServices;

namespace Annium;

public static class ArrayExtensions
{
    /// <summary>
    /// Allow the up to the first eight elements of an array to take part in C# 7's destructuring syntax.
    /// </summary>
    /// <example>
    /// (int first, _, int middle, _, int[] rest) = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
    /// var (first, second, rest) = new[] { 1, 2, 3, 4 };
    /// </example>
    public static void Deconstruct<T>(this T[] array, out T x1, out T[] rest)
    {
        x1 = array[0];
        rest = GetRestOfArray(array, 1);
    }

    public static void Deconstruct<T>(this T[] array, out T x1, out T x2, out T[] rest)
    {
        x1 = array[0];
        x2 = array[1];
        rest = GetRestOfArray(array, 2);
    }

    public static void Deconstruct<T>(this T[] array, out T x1, out T x2, out T x3, out T[] rest)
    {
        x1 = array[0];
        x2 = array[1];
        x3 = array[2];
        rest = GetRestOfArray(array, 3);
    }

    public static void Deconstruct<T>(this T[] array, out T x1, out T x2, out T x3, out T x4, out T[] rest)
    {
        x1 = array[0];
        x2 = array[1];
        x3 = array[2];
        x4 = array[3];
        rest = GetRestOfArray(array, 4);
    }

    public static void Deconstruct<T>(this T[] array, out T x1, out T x2, out T x3, out T x4, out T x5, out T[] rest)
    {
        x1 = array[0];
        x2 = array[1];
        x3 = array[2];
        x4 = array[3];
        x5 = array[4];
        rest = GetRestOfArray(array, 5);
    }

    public static void Deconstruct<T>(this T[] array, out T x1, out T x2, out T x3, out T x4, out T x5, out T x6, out T[] rest)
    {
        x1 = array[0];
        x2 = array[1];
        x3 = array[2];
        x4 = array[3];
        x5 = array[4];
        x6 = array[5];
        rest = GetRestOfArray(array, 6);
    }

    public static void Deconstruct<T>(this T[] array, out T x1, out T x2, out T x3, out T x4, out T x5, out T x6, out T x7, out T[] rest)
    {
        x1 = array[0];
        x2 = array[1];
        x3 = array[2];
        x4 = array[3];
        x5 = array[4];
        x6 = array[5];
        x7 = array[6];
        rest = GetRestOfArray(array, 7);
    }

    public static void Deconstruct<T>(this T[] array, out T x1, out T x2, out T x3, out T x4, out T x5, out T x6, out T x7, out T x8, out T[] rest)
    {
        x1 = array[0];
        x2 = array[1];
        x3 = array[2];
        x4 = array[3];
        x5 = array[4];
        x6 = array[5];
        x7 = array[6];
        x8 = array[7];
        rest = GetRestOfArray(array, 8);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T[] GetRestOfArray<T>(T[] array, int skip)
    {
        return array.Skip(skip).ToArray();
    }
}