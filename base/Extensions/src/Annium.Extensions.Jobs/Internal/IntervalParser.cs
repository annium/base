using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Annium.NodaTime.Extensions;
using NodaTime;

namespace Annium.Extensions.Jobs.Internal;

internal class IntervalParser : IIntervalParser
{
    private const string EveryMoment = "*";

    public Func<LocalDateTime, Duration> GetDelayResolver(string interval)
    {
        if (interval == Interval.Secondly)
            return _ => Duration.Zero;

        var intervals = interval.Split(' ').Select(e => e.Trim()).ToArray();
        if (intervals.Length != 5)
            throw new ArgumentException("Interval format has 5 parts: second minute hour day day-of-week");

        if (intervals[3] != EveryMoment && intervals[4] != EveryMoment)
            throw new ArgumentException($"Interval format {interval} must not have conditions on both day and day-of-week parts");

        var dateTime = Expression.Parameter(typeof(LocalDateTime), "x");
        var expressions = new List<Expression>();

        var result = Expression.Variable(typeof(Duration), "result");
        // expressions.Add(result);
        // expressions.Add(Expression.Assign(result, Expression.Constant(Duration.Zero)));

        // seconds
        HandleRepeatablePartExpression(
            Property(dateTime, nameof(LocalDateTime.Second)),
            FromLong(nameof(Duration.FromSeconds)),
            "second", intervals[0], 0, 59, 60,
            expressions.Add, result, Ceil(dateTime, nameof(LocalDateTimeExtensions.CeilToMinute)));

        // minutes
        HandleRepeatablePartExpression(
            Property(dateTime, nameof(LocalDateTime.Minute)),
            FromLong(nameof(Duration.FromMinutes)),
            "minute", intervals[1], 0, 59, 60,
            expressions.Add, result, Ceil(dateTime, nameof(LocalDateTimeExtensions.CeilToHour)));

        // days
        HandleRepeatablePartExpression(
            Property(dateTime, nameof(LocalDateTime.Hour)),
            FromInt(nameof(Duration.FromHours)),
            "hour", intervals[2], 0, 23, 24,
            expressions.Add, result, Ceil(dateTime, nameof(LocalDateTimeExtensions.CeilToDay)));

        if (intervals[4] == EveryMoment)
        {
            // day of month
            HandleRepeatablePartExpression(
                Property(dateTime, nameof(LocalDateTime.Day)),
                FromInt(nameof(Duration.FromDays)),
                "day", intervals[3], 0, 29, 30,
                expressions.Add, result, null);
        }
        else
        {
            // day of week
            HandleExactPartExpression(
                Expression.Convert(Property(dateTime, nameof(LocalDateTime.DayOfWeek)), typeof(int)),
                FromInt(nameof(Duration.FromDays)),
                "day of week", intervals[4], 1, 7, 7,
                expressions.Add, result, null);
        }

        var returnLabel = Expression.Label(typeof(Duration));
        expressions.Add(Expression.Return(returnLabel, result));
        expressions.Add(Expression.Label(returnLabel, result));

        var lambda = Expression.Lambda(Expression.Block(new[] { result }, expressions), false, dateTime);
        var resolver = (Func<LocalDateTime, Duration>)lambda.Compile();

        return resolver;
    }

    private void HandleRepeatablePartExpression(
        Expression source,
        Func<Expression, Expression> from,
        string name,
        string interval,
        int min,
        int max,
        int size,
        Action<Expression> add,
        Expression sum,
        Expression? align
    )
    {
        // if every interval - return null
        if (interval == "*")
            return;

        // if every X
        if (TryAsModulo(source, from, name, interval, min, max, size, add, sum, align))
            return;

        // if at constant X
        if (TryAsConst(source, from, name, interval, min, max, size, add, sum, align))
            return;

        // if at one of X
        if (TryAsList(source, from, name, interval, min, max, size, add, sum, align))
            return;

        throw new InvalidOperationException($"Can't handle scheduler interval part '{interval}'");
    }

    private void HandleExactPartExpression(
        Expression property,
        Func<Expression, Expression> from,
        string name,
        string interval,
        int min,
        int max,
        int size,
        Action<Expression> add,
        Expression sum,
        Expression? align
    )
    {
        // if every interval - return null
        if (interval == "*")
            return;

        // if at constant X
        if (TryAsConst(property, from, name, interval, min, max, size, add, sum, align))
            return;

        // if at one of X
        if (TryAsList(property, from, name, interval, min, max, size, add, sum, align))
            return;

        throw new InvalidOperationException($"Can't handle scheduler interval part '{interval}'");
    }

    private bool TryAsModulo(
        Expression source,
        Func<Expression, Expression> from,
        string name,
        string interval,
        int min,
        int max,
        int size,
        Action<Expression> add,
        Expression sum,
        Expression? align
    )
    {
        if (!interval.StartsWith("*/"))
            return false;

        var moduloString = interval[2..];
        if (!int.TryParse(moduloString, out var value))
            throw new ArgumentException($"'{name}' modulo {value} is not valid");

        if (value < min || value > max)
            throw new ArgumentOutOfRangeException(nameof(interval), value, $"'{name}' modulo {value} must be in [{min};{max}] range");

        if (size % value != 0)
            throw new ArgumentOutOfRangeException(nameof(interval), value, $"'{name}' modulo {value} is not acceptable for range {size}");

        var assignment = Expression.AddAssign(sum, from(Expression.Condition(
            Expression.Equal(Expression.Modulo(source, Expression.Constant(value)), Expression.Constant(0)),
            Expression.Constant(0),
            Expression.Subtract(
                Expression.Constant(value),
                Expression.Modulo(source, Expression.Constant(value))
            )
        )));
        add(assignment);
        if (align is not null)
            add(Expression.IfThen(Expression.GreaterThan(source, Expression.Constant(size - value)), align));

        return true;
    }

    private bool TryAsConst(
        Expression source,
        Func<Expression, Expression> from,
        string name,
        string interval,
        int min,
        int max,
        int size,
        Action<Expression> add,
        Expression sum,
        Expression? align
    )
    {
        if (!int.TryParse(interval, out var value))
            return false;

        if (value < min || value > max)
            throw new ArgumentOutOfRangeException(nameof(interval), value, $"'{name}' value {value} must be in [{min};{max}] range");

        var assignment = Expression.AddAssign(sum, from(Expression.Condition(
            Expression.LessThan(Expression.Constant(value), source),
            Expression.Subtract(Expression.Constant(value + size), source),
            Expression.Subtract(Expression.Constant(value), source)
        )));
        add(assignment);
        if (align is not null)
            add(Expression.IfThen(Expression.GreaterThan(source, Expression.Constant(value)), align));

        return true;
    }

    private bool TryAsList(
        Expression source,
        Func<Expression, Expression> from,
        string name,
        string interval,
        int min,
        int max,
        int size,
        Action<Expression> add,
        Expression sum,
        Expression? align
    )
    {
        var values = new HashSet<int>();
        foreach (var raw in interval.Split(','))
        {
            if (!int.TryParse(raw, out var value))
                return false;

            if (value < min || value > max)
                throw new ArgumentOutOfRangeException(nameof(interval), value, $"'{name}' value {value} must be in [{min};{max}] range");

            values.Add(value);
        }

        if (values.Count < 2)
            return false;

        var (last, rest) = values.OrderByDescending(x => x).ToArray();
        Expression res = Expression.Condition(
            Expression.LessThanOrEqual(source, Expression.Constant(last)),
            Expression.Subtract(Expression.Constant(last), source),
            Expression.Subtract(Expression.Constant(rest[^1] + size), source)
        );
        foreach (var point in rest)
            res = Expression.Condition(
                Expression.LessThanOrEqual(source, Expression.Constant(point)),
                Expression.Subtract(Expression.Constant(point), source),
                res
            );

        var assignment = Expression.AddAssign(sum, from(res));
        add(assignment);
        if (align is not null)
            add(Expression.IfThen(Expression.GreaterThan(source, Expression.Constant(last)), align));

        return true;
    }

    private static MemberExpression Property(ParameterExpression ex, string name) =>
        Expression.Property(ex, typeof(LocalDateTime).GetProperty(name)!);

    private static Func<Expression, Expression> FromInt(string name) =>
        ex => Expression.Call(null, typeof(Duration).GetMethod(name, new[] { typeof(int) })!, ex);

    private static Func<Expression, Expression> FromLong(string name) =>
        ex => Expression.Call(null, typeof(Duration).GetMethod(name, new[] { typeof(long) })!, Expression.Convert(ex, typeof(long)));

    private static Expression Ceil(ParameterExpression ex, string name) =>
        Expression.Assign(ex, Expression.Call(null, typeof(LocalDateTimeExtensions).GetMethod(name, new[] { ex.Type })!, ex));
}