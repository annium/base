using System.Linq;
using System.Runtime.CompilerServices;

namespace Annium;

/// <summary>
/// Provides extension methods for array destructuring and manipulation.
/// </summary>
public static class ArrayExtensions
{
    /// <summary>
    /// Allow the up to the first eight elements of an array to take part in C# 7's destructuring syntax.
    /// </summary>
    /// <example>
    /// (int first, _, int middle, _, int[] rest) = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
    /// var (first, second, rest) = new[] { 1, 2, 3, 4 };
    /// </example>
    /// <typeparam name="T">The type of the array elements.</typeparam>
    /// <param name="array">The source array.</param>
    /// <param name="x1">The first element of the array.</param>
    /// <param name="rest">The rest of the array.</param>
    public static void Deconstruct<T>(this T[] array, out T x1, out T[] rest)
    {
        x1 = array[0];
        rest = GetRestOfArray(array, 1);
    }

    /// <summary>
    /// Deconstructs the array into the first two elements and the rest of the array.
    /// </summary>
    /// <typeparam name="T">The type of the array elements.</typeparam>
    /// <param name="array">The source array.</param>
    /// <param name="x1">The first element of the array.</param>
    /// <param name="x2">The second element of the array.</param>
    /// <param name="rest">The rest of the array.</param>
    public static void Deconstruct<T>(this T[] array, out T x1, out T x2, out T[] rest)
    {
        x1 = array[0];
        x2 = array[1];
        rest = GetRestOfArray(array, 2);
    }

    /// <summary>
    /// Deconstructs the array into the first three elements and the rest of the array.
    /// </summary>
    /// <typeparam name="T">The type of the array elements.</typeparam>
    /// <param name="array">The source array.</param>
    /// <param name="x1">The first element of the array.</param>
    /// <param name="x2">The second element of the array.</param>
    /// <param name="x3">The third element of the array.</param>
    /// <param name="rest">The rest of the array.</param>
    public static void Deconstruct<T>(this T[] array, out T x1, out T x2, out T x3, out T[] rest)
    {
        x1 = array[0];
        x2 = array[1];
        x3 = array[2];
        rest = GetRestOfArray(array, 3);
    }

    /// <summary>
    /// Deconstructs the array into the first four elements and the rest of the array.
    /// </summary>
    /// <typeparam name="T">The type of the array elements.</typeparam>
    /// <param name="array">The source array.</param>
    /// <param name="x1">The first element of the array.</param>
    /// <param name="x2">The second element of the array.</param>
    /// <param name="x3">The third element of the array.</param>
    /// <param name="x4">The fourth element of the array.</param>
    /// <param name="rest">The rest of the array.</param>
    public static void Deconstruct<T>(this T[] array, out T x1, out T x2, out T x3, out T x4, out T[] rest)
    {
        x1 = array[0];
        x2 = array[1];
        x3 = array[2];
        x4 = array[3];
        rest = GetRestOfArray(array, 4);
    }

    /// <summary>
    /// Deconstructs the array into the first five elements and the rest of the array.
    /// </summary>
    /// <typeparam name="T">The type of the array elements.</typeparam>
    /// <param name="array">The source array.</param>
    /// <param name="x1">The first element of the array.</param>
    /// <param name="x2">The second element of the array.</param>
    /// <param name="x3">The third element of the array.</param>
    /// <param name="x4">The fourth element of the array.</param>
    /// <param name="x5">The fifth element of the array.</param>
    /// <param name="rest">The rest of the array.</param>
    public static void Deconstruct<T>(this T[] array, out T x1, out T x2, out T x3, out T x4, out T x5, out T[] rest)
    {
        x1 = array[0];
        x2 = array[1];
        x3 = array[2];
        x4 = array[3];
        x5 = array[4];
        rest = GetRestOfArray(array, 5);
    }

    /// <summary>
    /// Deconstructs the array into the first six elements and the rest of the array.
    /// </summary>
    /// <typeparam name="T">The type of the array elements.</typeparam>
    /// <param name="array">The source array.</param>
    /// <param name="x1">The first element of the array.</param>
    /// <param name="x2">The second element of the array.</param>
    /// <param name="x3">The third element of the array.</param>
    /// <param name="x4">The fourth element of the array.</param>
    /// <param name="x5">The fifth element of the array.</param>
    /// <param name="x6">The sixth element of the array.</param>
    /// <param name="rest">The rest of the array.</param>
    public static void Deconstruct<T>(
        this T[] array,
        out T x1,
        out T x2,
        out T x3,
        out T x4,
        out T x5,
        out T x6,
        out T[] rest
    )
    {
        x1 = array[0];
        x2 = array[1];
        x3 = array[2];
        x4 = array[3];
        x5 = array[4];
        x6 = array[5];
        rest = GetRestOfArray(array, 6);
    }

    /// <summary>
    /// Deconstructs the array into the first seven elements and the rest of the array.
    /// </summary>
    /// <typeparam name="T">The type of the array elements.</typeparam>
    /// <param name="array">The source array.</param>
    /// <param name="x1">The first element of the array.</param>
    /// <param name="x2">The second element of the array.</param>
    /// <param name="x3">The third element of the array.</param>
    /// <param name="x4">The fourth element of the array.</param>
    /// <param name="x5">The fifth element of the array.</param>
    /// <param name="x6">The sixth element of the array.</param>
    /// <param name="x7">The seventh element of the array.</param>
    /// <param name="rest">The rest of the array.</param>
    public static void Deconstruct<T>(
        this T[] array,
        out T x1,
        out T x2,
        out T x3,
        out T x4,
        out T x5,
        out T x6,
        out T x7,
        out T[] rest
    )
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

    /// <summary>
    /// Deconstructs the array into the first eight elements and the rest of the array.
    /// </summary>
    /// <typeparam name="T">The type of the array elements.</typeparam>
    /// <param name="array">The source array.</param>
    /// <param name="x1">The first element of the array.</param>
    /// <param name="x2">The second element of the array.</param>
    /// <param name="x3">The third element of the array.</param>
    /// <param name="x4">The fourth element of the array.</param>
    /// <param name="x5">The fifth element of the array.</param>
    /// <param name="x6">The sixth element of the array.</param>
    /// <param name="x7">The seventh element of the array.</param>
    /// <param name="x8">The eighth element of the array.</param>
    /// <param name="rest">The rest of the array.</param>
    public static void Deconstruct<T>(
        this T[] array,
        out T x1,
        out T x2,
        out T x3,
        out T x4,
        out T x5,
        out T x6,
        out T x7,
        out T x8,
        out T[] rest
    )
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

    /// <summary>
    /// Returns the rest of the array after skipping a specified number of elements.
    /// </summary>
    /// <typeparam name="T">The type of the array elements.</typeparam>
    /// <param name="array">The source array.</param>
    /// <param name="skip">The number of elements to skip.</param>
    /// <returns>An array containing the elements after the skipped ones.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T[] GetRestOfArray<T>(T[] array, int skip)
    {
        return array.Skip(skip).ToArray();
    }
}
