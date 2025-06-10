using System.Linq.Expressions;

namespace Annium.Core.Mapper;

/// <summary>
/// Delegate for mapping expressions from one form to another
/// </summary>
/// <param name="ex">The input expression</param>
/// <returns>The transformed expression</returns>
public delegate Expression Mapping(Expression ex);
