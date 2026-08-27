using System;
using System.Threading.Tasks;
using Annium.Testing;
using Xunit;

namespace Annium.Extensions.Validation.Tests.Rules;

/// <summary>
/// Tests the comparison rules against a type whose CompareTo returns a difference rather than -1/0/1.
/// IComparable only promises the sign of the result, and returning the difference is a common way to
/// implement it, so a rule that reads the value itself rather than its sign silently accepts anything.
/// </summary>
public class ComparableRulesTest : TestBase
{
    /// <summary>
    /// A value below the minimum is rejected however far below it is.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Between_BelowMinimumByMoreThanOne_Fails()
    {
        // arrange
        var validator = GetValidator<Order>();

        // act - twelve units below the minimum, so CompareTo returns -1200, not -1
        var result = await validator.ValidateAsync(new Order { Total = new Money(-1200) });

        // assert
        result.LabeledErrors.At(nameof(Order.Total)).At(0).Is("Value is less, than given minimum");
    }

    /// <summary>
    /// A value above the maximum is rejected however far above it is.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Between_AboveMaximumByMoreThanOne_Fails()
    {
        // arrange
        var validator = GetValidator<Order>();

        // act
        var result = await validator.ValidateAsync(new Order { Total = new Money(9900) });

        // assert
        result.LabeledErrors.At(nameof(Order.Total)).At(0).Is("Value is greater, than given maximum");
    }

    /// <summary>
    /// A value inside the range still passes.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Between_InRange_Passes()
    {
        // arrange
        var validator = GetValidator<Order>();

        // act
        var result = await validator.ValidateAsync(new Order { Total = new Money(500) });

        // assert
        result.HasErrors.IsFalse();
    }
}

/// <summary>
/// An amount in cents, comparing by difference - a legal IComparable implementation that returns
/// magnitudes rather than -1/0/1.
/// </summary>
/// <param name="Cents">The amount, in cents.</param>
public record Money(int Cents) : IComparable<Money>
{
    /// <summary>
    /// Compares this amount to another, returning their difference.
    /// </summary>
    /// <param name="other">The amount to compare against.</param>
    /// <returns>The difference between the two amounts, in cents.</returns>
    public int CompareTo(Money? other) => other is null ? 1 : Cents - other.Cents;
}

/// <summary>
/// Model carrying an amount constrained to a range.
/// </summary>
public class Order
{
    /// <summary>
    /// Gets or sets the order total.
    /// </summary>
    public Money Total { get; set; } = new(0);
}

/// <summary>
/// Validator constraining the order total to a range.
/// </summary>
public class OrderValidator : Validator<Order>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OrderValidator"/> class.
    /// </summary>
    public OrderValidator()
    {
        Field(x => x.Total).Between(new Money(0), new Money(1000));
    }
}
