using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using NodaTime;

namespace Annium.Extensions.Jobs.Internal;

/// <summary>
/// Resolves cron expressions into compiled matching functions using expression trees for optimal performance
/// </summary>
internal class IntervalResolver : IIntervalResolver
{
    /// <summary>
    /// Creates a compiled function that determines if a given instant matches the cron expression
    /// </summary>
    /// <param name="interval">The cron expression with 5 parts: minute hour day month dayOfWeek</param>
    /// <returns>A compiled function that returns true if the instant matches the schedule</returns>
    public Func<Instant, bool> GetMatcher(string interval)
    {
        var intervals = interval.Split(' ').Select(e => e.Trim()).ToArray();
        if (intervals.Length != 5)
            throw new ArgumentException("Interval format has 5 parts: minute hour day month dayOfWeek");

        var expressions = new List<Expression>();

        var instant = Expression.Parameter(typeof(Instant));

        // declare ZonedDateTime variable
        var zonedTime = Expression.Variable(typeof(ZonedDateTime));

        // convert instant to it
        expressions.Add(
            Expression.Assign(zonedTime, Expression.Call(instant, typeof(Instant).GetMethod(nameof(Instant.InUtc))!))
        );

        var type = typeof(ZonedDateTime);
        var parts = new[]
        {
            GetPartExpression("second", "0", 0, 59, ZonedDateTimeProperty(zonedTime, nameof(ZonedDateTime.Second))),
            GetPartExpression(
                "minute",
                intervals[0],
                0,
                59,
                ZonedDateTimeProperty(zonedTime, nameof(ZonedDateTime.Minute))
            ),
            GetPartExpression(
                "hour",
                intervals[1],
                0,
                23,
                ZonedDateTimeProperty(zonedTime, nameof(ZonedDateTime.Hour))
            ),
            GetPartExpression("day", intervals[2], 0, 30, ZonedDateTimeProperty(zonedTime, nameof(ZonedDateTime.Day))),
            GetPartExpression(
                "month",
                intervals[3],
                0,
                11,
                ZonedDateTimeProperty(zonedTime, nameof(ZonedDateTime.Month))
            ),
            GetPartExpression(
                "day of week",
                intervals[4],
                1,
                7,
                Expression.Convert(
                    Expression.Property(zonedTime, type.GetProperty(nameof(ZonedDateTime.DayOfWeek))!),
                    typeof(int)
                )
            ),
        }
            .OfType<Expression>()
            .ToArray();

        var match = parts.Length == 0 ? Expression.Constant(true) : parts[0];
        foreach (var part in parts.Skip(1))
            match = Expression.And(match, part);

        expressions.Add(match);

        var expression = Expression.Lambda(Expression.Block(new[] { zonedTime }, expressions), false, instant);

        return (Func<Instant, bool>)expression.Compile();
    }

    /// <summary>
    /// Creates an expression that matches a specific time part against the cron expression part
    /// </summary>
    /// <param name="name">The name of the time part for error messages</param>
    /// <param name="part">The cron expression part to match against</param>
    /// <param name="min">Minimum valid value for this time part</param>
    /// <param name="max">Maximum valid value for this time part</param>
    /// <param name="getPart">Expression that gets the current value of this time part</param>
    /// <returns>An expression that evaluates to true if the time part matches, null if wildcard</returns>
    private Expression? GetPartExpression(string name, string part, uint min, uint max, Expression getPart)
    {
        // if any - return null
        if (part == "*")
            return null;

        // if const - test equality
        if (uint.TryParse(part, out var value))
            if (value >= min && value <= max)
                return Expression.Equal(getPart, Expression.Constant((int)value));
            else
                throw new ArgumentOutOfRangeException(nameof(part), value, $"'{name}' must be in [{min};{max}] range");

        // if list - handle with "or" equality
        if (Regex.IsMatch(part, "([0-9]{1,2})(?:,[0-9]{1,2})+"))
        {
            var values = part.Split(',').Select(uint.Parse).ToArray();
            if (values.Any(v => v < min || v > max))
                throw new ArgumentOutOfRangeException(nameof(part), $"'{name}' must be in [{min};{max}] range");

            var result = Expression.Equal(getPart, Expression.Constant((int)values[0]));
            foreach (var orValue in values.Skip(1))
                result = Expression.Or(result, Expression.Equal(getPart, Expression.Constant((int)orValue)));

            return result;
        }

        throw new InvalidOperationException($"Can't handle scheduler interval part '{part}'");
    }

    /// <summary>
    /// Creates a property access expression for ZonedDateTime properties
    /// </summary>
    /// <param name="ex">The parameter expression representing the ZonedDateTime instance</param>
    /// <param name="name">The property name to access</param>
    /// <returns>A member expression for the property access</returns>
    private MemberExpression ZonedDateTimeProperty(ParameterExpression ex, string name) =>
        Expression.Property(ex, typeof(ZonedDateTime).GetProperty(name)!);
}
