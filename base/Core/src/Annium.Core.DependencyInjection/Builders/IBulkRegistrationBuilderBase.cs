using System;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

/// <summary>
/// Base interface for bulk registration builder.
/// </summary>
public interface IBulkRegistrationBuilderBase : IBulkRegistrationBuilderTarget
{
    /// <summary>
    /// Filters types for registration.
    /// </summary>
    /// <param name="predicate">Type filter predicate.</param>
    /// <returns>Builder with the applied filter.</returns>
    IBulkRegistrationBuilderBase Where(Func<Type, bool> predicate);
}
