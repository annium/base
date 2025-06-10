using System.Linq.Expressions;

namespace Annium.Core.Mapper;

/// <summary>
/// Repacks expressions into mapping configurations
/// </summary>
public interface IRepacker
{
    /// <summary>
    /// Repacks an expression into a mapping configuration
    /// </summary>
    /// <param name="ex">The expression to repack</param>
    /// <returns>The repacked mapping</returns>
    Mapping Repack(Expression ex);
}
