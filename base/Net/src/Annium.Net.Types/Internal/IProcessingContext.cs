using System;
using System.Collections.Generic;
using Annium.Net.Types.Internal.Config;
using Annium.Net.Types.Models;
using Annium.Net.Types.Refs;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal;

/// <summary>
/// Extended processing context interface specific to model mapping operations.
/// Provides access to generated models in addition to standard processing capabilities.
/// </summary>
internal interface IMapperProcessingContext : IProcessingContext
{
    /// <summary>
    /// Gets all models that have been created during the mapping process.
    /// </summary>
    /// <returns>A read-only collection of all generated type models</returns>
    IReadOnlyCollection<IModel> GetModels();
}

/// <summary>
/// Core processing context interface for type mapping operations.
/// Manages type processing, reference resolution, and model registration.
/// </summary>
internal interface IProcessingContext
{
    /// <summary>
    /// Gets the internal mapper configuration.
    /// </summary>
    IMapperConfigInternal Config { get; }

    /// <summary>
    /// Gets all implementations for the specified contextual type.
    /// </summary>
    /// <param name="type">The contextual type to find implementations for</param>
    /// <returns>A collection of contextual types representing implementations</returns>
    IReadOnlyCollection<ContextualType> GetImplementations(ContextualType type);

    /// <summary>
    /// Processes a contextual type with explicit nullability information.
    /// </summary>
    /// <param name="type">The contextual type to process</param>
    /// <param name="nullability">The nullability context</param>
    void Process(ContextualType type, Nullability nullability);

    /// <summary>
    /// Processes a contextual type using its inherent nullability context.
    /// </summary>
    /// <param name="type">The contextual type to process</param>
    void Process(ContextualType type);

    /// <summary>
    /// Gets the type reference for a contextual type with explicit nullability.
    /// </summary>
    /// <param name="type">The contextual type</param>
    /// <param name="nullability">The nullability context</param>
    /// <returns>The type reference for the specified type</returns>
    IRef GetRef(ContextualType type, Nullability nullability);

    /// <summary>
    /// Gets the type reference for a contextual type using its inherent nullability.
    /// </summary>
    /// <param name="type">The contextual type</param>
    /// <returns>The type reference for the specified type</returns>
    IRef GetRef(ContextualType type);

    /// <summary>
    /// Gets the type reference for a contextual type, ensuring it exists.
    /// </summary>
    /// <param name="type">The contextual type</param>
    /// <returns>The type reference for the specified type</returns>
    IRef RequireRef(ContextualType type);

    /// <summary>
    /// Determines whether a model is registered for the specified type.
    /// </summary>
    /// <param name="type">The type to check</param>
    /// <returns>True if a model is registered for the type, false otherwise</returns>
    bool IsRegistered(Type type);

    /// <summary>
    /// Registers a model for the specified type.
    /// </summary>
    /// <param name="type">The type to register the model for</param>
    /// <param name="model">The model to register</param>
    void Register(Type type, IModel model);
}
