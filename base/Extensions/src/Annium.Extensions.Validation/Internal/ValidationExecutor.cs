using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection.Extensions;
using Annium.Data.Operations;
using Annium.Localization.Abstractions;
using Annium.Reflection.Types;

namespace Annium.Extensions.Validation.Internal;

/// <summary>
/// Internal validation executor that orchestrates validation across multiple validators and inheritance hierarchies.
/// Supports multi-stage validation execution and localized error messages.
/// </summary>
/// <typeparam name="TValue">The type of the value being validated</typeparam>
internal class ValidationExecutor<TValue> : IValidator<TValue>
{
    /// <summary>
    /// Pre-computed array of validator set types for the value type and its inheritance hierarchy
    /// </summary>
    private static readonly Type[] _validatorSets = typeof(TValue)
        .GetInheritanceChain(self: true)
        .Concat(typeof(TValue).GetInterfaces())
        .Select(t => typeof(IEnumerable<>).MakeGenericType(typeof(IValidationContainer<>).MakeGenericType(t)))
        .ToArray();

    /// <summary>
    /// Array of all validation containers that apply to this value type
    /// </summary>
    private readonly IValidationContainer<TValue>[] _validators;

    /// <summary>
    /// Localizer for translating validation error messages
    /// </summary>
    private readonly ILocalizer<TValue> _localizer;

    /// <summary>
    /// Initializes a new validation executor by resolving all applicable validators from the service provider
    /// </summary>
    /// <param name="serviceProvider">Service provider to resolve validators and localizer from</param>
    public ValidationExecutor(IServiceProvider serviceProvider)
    {
        _validators = _validatorSets
            .Select(s => (IEnumerable<IValidationContainer<TValue>>)serviceProvider.Resolve(s))
            .SelectMany(v => v)
            .ToArray();

        _localizer = serviceProvider.Resolve<ILocalizer<TValue>>();
    }

    /// <summary>
    /// Validates the specified value using all registered validators in a multi-stage execution pattern
    /// </summary>
    /// <param name="value">The value to validate</param>
    /// <param name="label">Optional label for error message context</param>
    /// <returns>Validation result containing any errors found</returns>
    public async Task<IResult> ValidateAsync(TValue value, string? label = null)
    {
        var hasLabel = !string.IsNullOrWhiteSpace(label);

        if (value == null)
            return hasLabel ? Result.New().Error(label!, "Value is null") : Result.New().Error("Value is null");

        if (_validators.Length == 0)
            return Result.New();

        var result = Result.New();
        var stage = 0;
        bool ranStage;
        do
        {
            ranStage = false;

            foreach (var validator in _validators)
            {
                var (validatorResult, hasRun) = await validator.ValidateAsync(
                    value,
                    label ?? string.Empty,
                    stage,
                    _localizer
                );

                result.Join(validatorResult);
                ranStage = hasRun || ranStage;
            }

            // short-circuit if any errors after stage execution
            if (result.HasErrors)
                return result;

            // go next stage, if there was any run on current
            stage++;
        } while (ranStage);

        return result;
    }
}
