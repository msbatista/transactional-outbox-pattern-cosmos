using Domain;
using Microsoft.Azure.Cosmos;

namespace Infrastructure.Context;

public interface IContainerContext
{
    Container Container { get; }
    List<IDataObject<Entity>> DataObjects { get; }
    void Add(IDataObject<Entity> entity);
    Task<List<IDataObject<Entity>>> SaveChangesAsync(CancellationToken cancellationToken = default);
    void Reset();
}
