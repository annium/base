namespace Annium.Data.Models;

public interface IIdEntity<TId>
    where TId : struct
{
    public TId Id { get; }
}