using System.Runtime.CompilerServices;

namespace Annium;

public static class CastExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T CastTo<T>(this object value) => (T)value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T? TryCastTo<T>(this object value) where T : class => value as T;
}