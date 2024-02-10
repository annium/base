namespace Annium.Data.Models;

public interface IIdEntity<TId>
    where TId : struct
{
    TId Id { get; }
}
