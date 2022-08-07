using System.Runtime.InteropServices.ComTypes;
using Domain;
using Infrastructure.Context;

namespace Infrastructure;

public interface IUnitOfWork
{
    IContactRepository ContactsRepository { get; }
    Task<List<IDataObject<Entity>>> CommitAsync(CancellationToken cancellationToken = default);
}
