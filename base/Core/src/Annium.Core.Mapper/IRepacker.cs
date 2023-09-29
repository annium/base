using System.Linq.Expressions;

namespace Annium.Core.Mapper;

public interface IRepacker
{
    Mapping Repack(Expression ex);
}