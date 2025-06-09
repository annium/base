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

/// <summary>
/// Internal processing context that manages type processing state and coordinates between processors and referrers.
/// </summary>
internal sealed record ProcessingContext : IMapperProcessingContext
{
    /// <summary>
    /// Gets the internal mapper configuration used for processing decisions.
    /// </summary>
    public IMapperConfigInternal Config { get; }

    /// <summary>
    /// Dictionary mapping types to their corresponding generated models.
    /// </summary>
    private readonly Dictionary<Type, IModel> _models = new();

    /// <summary>
    /// The processor responsible for handling type processing logic.
    /// </summary>
    private readonly Processor _processor;

    /// <summary>
    /// The referrer responsible for creating type references.
    /// </summary>
    private readonly Referrer _referrer;

    /// <summary>
    /// The type manager for runtime type operations.
    /// </summary>
    private readonly ITypeManager _typeManager;

    /// <summary>
    /// Lazy initialization for processing explicitly included types.
    /// </summary>
    private readonly Lazy<None> _processIncluded;

    /// <summary>
    /// Initializes a new processing context with the specified components.
    /// </summary>
    /// <param name="config">The internal mapper configuration</param>
    /// <param name="processor">The processor for handling type processing</param>
    /// <param name="referrer">The referrer for creating type references</param>
    /// <param name="typeManager">The type manager for runtime operations</param>
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

    /// <summary>
    /// Gets all implementation types for the specified contextual type.
    /// </summary>
    /// <param name="type">The contextual type to get implementations for</param>
    /// <returns>A collection of contextual types representing the implementations</returns>
    public IReadOnlyCollection<ContextualType> GetImplementations(ContextualType type) =>
        _typeManager.GetImplementations(type.Type).Select(x => x.ToContextualType()).ToArray();

    /// <summary>
    /// Processes the specified contextual type, generating any necessary models.
    /// </summary>
    /// <param name="type">The contextual type to process</param>
    public void Process(ContextualType type) => _processor.Process(type, this);

    /// <summary>
    /// Processes the specified contextual type with explicit nullability information.
    /// </summary>
    /// <param name="type">The contextual type to process</param>
    /// <param name="nullability">The nullability information for the type</param>
    public void Process(ContextualType type, Nullability nullability) => _processor.Process(type, nullability, this);

    /// <summary>
    /// Gets a type reference for the specified contextual type.
    /// </summary>
    /// <param name="type">The contextual type to get a reference for</param>
    /// <returns>A type reference representing the contextual type</returns>
    public IRef GetRef(ContextualType type) => _referrer.GetRef(type, this);

    /// <summary>
    /// Gets a type reference for the specified contextual type with explicit nullability.
    /// </summary>
    /// <param name="type">The contextual type to get a reference for</param>
    /// <param name="nullability">The nullability information for the type</param>
    /// <returns>A type reference representing the contextual type</returns>
    public IRef GetRef(ContextualType type, Nullability nullability) => _referrer.GetRef(type, nullability, this);

    /// <summary>
    /// Gets a required type reference for the specified contextual type, throwing if no model exists.
    /// </summary>
    /// <param name="type">The contextual type to get a reference for</param>
    /// <returns>A type reference representing the contextual type</returns>
    public IRef RequireRef(ContextualType type)
    {
        var model =
            _models.GetValueOrDefault(type.Type)
            ?? throw new InvalidOperationException($"No model is registered for type {type.Type}");

        return model switch
        {
            EnumModel x => new EnumRef(x.Namespace.ToString(), x.Name),
            InterfaceModel x => new InterfaceRef(x.Namespace.ToString(), x.Name, x.Args.ToArray()),
            StructModel x => new StructRef(x.Namespace.ToString(), x.Name, x.Args.ToArray()),
            _ => throw new ArgumentOutOfRangeException($"Unexpected model {model}"),
        };
    }

    /// <summary>
    /// Determines whether a model has been registered for the specified type.
    /// </summary>
    /// <param name="type">The type to check for registration</param>
    /// <returns>True if a model is registered for the type, false otherwise</returns>
    public bool IsRegistered(Type type)
    {
        return _models.ContainsKey(type);
    }

    /// <summary>
    /// Registers a model for the specified type.
    /// </summary>
    /// <param name="type">The type to register the model for</param>
    /// <param name="model">The model to register</param>
    public void Register(Type type, IModel model)
    {
        _models.TryAdd(type, model);
    }

    /// <summary>
    /// Gets all registered models, ensuring included types are processed first.
    /// </summary>
    /// <returns>A read-only collection of all registered models</returns>
    public IReadOnlyCollection<IModel> GetModels()
    {
        var _ = _processIncluded.Value;

        return _models.Values.ToArray();
    }

    /// <summary>
    /// Processes all explicitly included types.
    /// </summary>
    /// <returns>None to indicate completion</returns>
    private None ProcessIncluded()
    {
        foreach (var type in Config.Included)
            Process(type.ToContextualType());

        return None.Default;
    }
}
