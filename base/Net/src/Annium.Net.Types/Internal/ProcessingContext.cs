using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Core.Runtime.Types;
using Annium.Net.Types.Internal.Config;
using Annium.Net.Types.Internal.Processors;
using Annium.Net.Types.Internal.Referrers;
using Annium.Net.Types.Models;
using Annium.Net.Types.Refs;
using Namotion.Reflection;

// ReSharper disable CoVariantArrayConversion

namespace Annium.Net.Types.Internal;

internal sealed record ProcessingContext : IMapperProcessingContext
{
    public IMapperConfigInternal Config { get; }
    private readonly Dictionary<Type, IModel> _models = new();
    private readonly Processor _processor;
    private readonly Referrer _referrer;
    private readonly ITypeManager _typeManager;
    private Lazy<None> _processIncluded;

    public ProcessingContext(
        IMapperConfigInternal config,
        Processor processor,
        Referrer referrer,
        ITypeManager typeManager
    )
    {
        Config = config;
        _processor = processor;
        _referrer = referrer;
        _typeManager = typeManager;
        _processIncluded = new Lazy<None>(ProcessIncluded);
    }

    public IReadOnlyCollection<ContextualType> GetImplementations(ContextualType type) => _typeManager
        .GetImplementations(type.Type)
        .Select(x => x.ToContextualType())
        .ToArray();

    public void Process(ContextualType type) => _processor.Process(type, this);
    public void Process(ContextualType type, Nullability nullability) => _processor.Process(type, nullability, this);
    public IRef GetRef(ContextualType type) => _referrer.GetRef(type, this);
    public IRef GetRef(ContextualType type, Nullability nullability) => _referrer.GetRef(type, nullability, this);

    public IRef RequireRef(ContextualType type)
    {
        var model = _models.GetValueOrDefault(type.Type) ?? throw new InvalidOperationException($"No model is registered for type {type.Type}");

        return model switch
        {
            EnumModel x      => new EnumRef(x.Namespace.ToString(), x.Name),
            InterfaceModel x => new InterfaceRef(x.Namespace.ToString(), x.Name, x.Args.ToArray()),
            StructModel x    => new StructRef(x.Namespace.ToString(), x.Name, x.Args.ToArray()),
            _                => throw new ArgumentOutOfRangeException($"Unexpected model {model}")
        };
    }

    public bool IsRegistered(Type type)
    {
        return _models.ContainsKey(type);
    }

    public void Register(Type type, IModel model)
    {
        _models.TryAdd(type, model);
    }

    public IReadOnlyCollection<IModel> GetModels()
    {
        var _ = _processIncluded.Value;

        return _models.Values.ToArray();
    }

    private None ProcessIncluded()
    {
        foreach (var type in Config.Included)
            Process(type.ToContextualType());

        return None.Default;
    }
}