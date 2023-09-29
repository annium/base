namespace Annium.Core.Mapper;

public interface IMapContext
{
    T Map<T>(object source);
}