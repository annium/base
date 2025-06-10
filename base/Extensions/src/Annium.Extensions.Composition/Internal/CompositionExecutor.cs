using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Core.DependencyInjection.Extensions;
using Annium.Data.Operations;
using Annium.Localization.Abstractions;
using Annium.Reflection.Types;

namespace Annium.Extensions.Composition.Internal;

/// <summary>
/// Executes composition operations by orchestrating multiple composition containers for a given value type
/// </summary>
/// <typeparam name="TValue">The type of value to compose</typeparam>
internal class CompositionExecutor<TValue> : IComposer<TValue>
    where TValue : class
{
    /// <summary>
    /// Static array of composer set types for resolving composition containers from the inheritance chain
    /// </summary>
    private static readonly Type[] _composerSets = typeof(TValue)
        .GetInheritanceChain(self: true)
        .Concat(typeof(TValue).GetInterfaces())
        .Select(t => typeof(IEnumerable<>).MakeGenericType(typeof(ICompositionContainer<>).MakeGenericType(t)))
        .ToArray();

    /// <summary>
    /// Array of composition containers that will be used to compose the value
    /// </summary>
    private readonly ICompositionContainer<TValue>[] _composers;

    /// <summary>
    /// Localizer for translating error messages specific to the value type
    /// </summary>
    private readonly ILocalizer<TValue> _localizer;

    /// <summary>
    /// Initializes a new instance of the CompositionExecutor class
    /// </summary>
    /// <param name="serviceProvider">The service provider for resolving dependencies</param>
    public CompositionExecutor(IServiceProvider serviceProvider)
    {
        _composers = _composerSets
            .Select(s => (IEnumerable<ICompositionContainer<TValue>>)serviceProvider.Resolve(s))
            .SelectMany(v => v)
            .ToArray();

        var duplicates = GetDuplicates(_composers);
        if (duplicates.Count > 0)
            throw new InvalidOperationException(
                $@"{typeof(TValue)} has {duplicates.Count} properties with multiple loaders:{Environment.NewLine}{string.Join(Environment.NewLine, duplicates.Select(p => $"{p.Key.Name}: {string.Join(", ", p.Value)}"))}"
            );

        _localizer = serviceProvider.Resolve<ILocalizer<TValue>>();
    }

    /// <summary>
    /// Composes the specified value by applying all registered composition containers
    /// </summary>
    /// <param name="value">The value to compose</param>
    /// <param name="label">Optional label for error reporting context</param>
    /// <returns>A status result indicating the success or failure of the composition operation</returns>
    public async Task<IStatusResult<OperationStatus>> ComposeAsync(TValue? value, string label = "")
    {
        var hasLabel = !string.IsNullOrWhiteSpace(label);

        if (value is null)
            return hasLabel
                ? Result.Status(OperationStatus.BadRequest).Error(label, "Value is null")
                : Result.Status(OperationStatus.BadRequest).Error("Value is null");

        if (_composers.Length == 0)
            return Result.Status(OperationStatus.Ok);

        var result = Result.New();

        foreach (var composer in _composers)
            result.Join(await composer.ComposeAsync(value, label, _localizer));

        return Result.Status(result.IsOk ? OperationStatus.Ok : OperationStatus.NotFound).Join(result);
    }

    /// <summary>
    /// Identifies duplicate field configurations across multiple composition containers
    /// </summary>
    /// <param name="composers">Array of composition containers to check for duplicates</param>
    /// <returns>Dictionary of properties that have multiple composers configured</returns>
    private IReadOnlyDictionary<PropertyInfo, IList<Type>> GetDuplicates(ICompositionContainer<TValue>[] composers)
    {
        var duplicates = new Dictionary<PropertyInfo, IList<Type>>();
        foreach (var composer in composers)
        foreach (var field in composer.Fields)
            if (duplicates.ContainsKey(field))
                duplicates[field].Add(composer.GetType());
            else
                duplicates[field] = new List<Type> { composer.GetType() };

        return duplicates.Where(p => p.Value.Count > 1).ToDictionary(p => p.Key, p => p.Value);
    }
}
