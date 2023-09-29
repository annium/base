using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Core.DependencyInjection;
using Annium.Data.Operations;
using Annium.Localization.Abstractions;
using Annium.Reflection;

namespace Annium.Extensions.Composition.Internal;

internal class CompositionExecutor<TValue> : IComposer<TValue> where TValue : class
{
    private static readonly Type[] ComposerSets = typeof(TValue)
        .GetInheritanceChain(self: true)
        .Concat(typeof(TValue).GetInterfaces())
        .Select(t => typeof(IEnumerable<>).MakeGenericType(typeof(ICompositionContainer<>).MakeGenericType(t)))
        .ToArray();

    private readonly ICompositionContainer<TValue>[] _composers;

    private readonly ILocalizer<TValue> _localizer;

    public CompositionExecutor(
        IServiceProvider serviceProvider
    )
    {
        _composers = ComposerSets
            .Select(s => (IEnumerable<ICompositionContainer<TValue>>)serviceProvider.Resolve(s))
            .SelectMany(v => v)
            .ToArray();

        var duplicates = GetDuplicates(_composers);
        if (duplicates.Count > 0)
            throw new InvalidOperationException(
                $@"{typeof(TValue)} has {duplicates.Count} properties with multiple loaders:{Environment.NewLine}{string.Join(Environment.NewLine, duplicates.Select(p => $"{p.Key.Name}: {string.Join(", ", p.Value)}"))}");

        _localizer = serviceProvider.Resolve<ILocalizer<TValue>>();
    }

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