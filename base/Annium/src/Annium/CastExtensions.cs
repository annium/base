using System.Runtime.CompilerServices;

namespace Annium;

/// <summary>
/// Provides extension methods for casting objects to a specified type.
/// </summary>
public static class CastExtensions
{
    /// <summary>
    /// Casts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to cast to.</typeparam>
    /// <param name="value">The object to cast.</param>
    /// <returns>The object cast to type <typeparamref name="T"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T CastTo<T>(this object value) => (T)value;

    /// <summary>
    /// Attempts to cast an object to the specified reference type.
    /// </summary>
    /// <typeparam name="T">The reference type to cast to.</typeparam>
    /// <param name="value">The object to cast.</param>
    /// <returns>The object cast to type <typeparamref name="T"/>, or null if the cast is not possible.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T? TryCastTo<T>(this object value)
        where T : class => value as T;
}
