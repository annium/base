using System;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

/// <summary>
/// Base interface for bulk registration builder.
/// </summary>
public interface IBulkRegistrationBuilderBase : IBulkRegistrationBuilderTarget
{
    /// <summary>
    /// Filter types for registration
    /// </summary>
    /// <param name="predicate">type filter</param>
    /// <returns>builder with applied filter</returns>
    IBulkRegistrationBuilderBase Where(Func<Type, bool> predicate);
}
