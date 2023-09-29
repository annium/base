using System;
using System.Linq;

namespace Annium;

public static class RandomExtensions
{
    public static bool NextBool(this Random random) => random.Next(0, 1) == 1;

    public static T NextEnum<T>(this Random random, params T[] values) where T : Enum
    {
        if (values.Length == 0)
            values = Enum.GetValues(typeof(T)).OfType<T>().ToArray();

        return values[random.Next(0, values.Length)];
    }

    public static decimal NextDecimal(this Random random) => (decimal)random.NextDouble();
}