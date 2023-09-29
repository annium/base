using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Data.Operations;
using Annium.Localization.Abstractions;
using Annium.Reflection;

namespace Annium.Extensions.Validation.Internal;

internal class ValidationExecutor<TValue> : IValidator<TValue>
{
    private static readonly Type[] ValidatorSets = typeof(TValue)
        .GetInheritanceChain(self: true)
        .Concat(typeof(TValue).GetInterfaces())
        .Select(t => typeof(IEnumerable<>).MakeGenericType(typeof(IValidationContainer<>).MakeGenericType(t)))
        .ToArray();

    private readonly IValidationContainer<TValue>[] _validators;

    private readonly ILocalizer<TValue> _localizer;

    public ValidationExecutor(
        IServiceProvider serviceProvider
    )
    {
        _validators = ValidatorSets
            .Select(s => (IEnumerable<IValidationContainer<TValue>>)serviceProvider.Resolve(s))
            .SelectMany(v => v)
            .ToArray();

        _localizer = serviceProvider.Resolve<ILocalizer<TValue>>();
    }

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
                var (validatorResult, hasRun) = await validator.ValidateAsync(value, label ?? string.Empty, stage, _localizer);

                result.Join(validatorResult);
                ranStage = hasRun || ranStage;
            }

            // short-circuit if any errors after stage execution
            if (result.HasErrors)
                return result;

            // go next stage, if there was any run on current
            stage++;
        }
        while (ranStage);

        return result;
    }
}