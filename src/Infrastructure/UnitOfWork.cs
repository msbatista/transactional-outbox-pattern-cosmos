using Domain;
using Infrastructure.Context;

namespace Infrastructure;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly IContainerContext _context;

    public UnitOfWork(IContainerContext context, IContactRepository contactsRepository)
    {
        _context = context;
        ContactsRepository = contactsRepository;
    }

    public IContactRepository ContactsRepository { get; }

    public Task<List<IDataObject<Entity>>> CommitAsync(CancellationToken cancellationToken = default) 
        => _context.SaveChangesAsync(cancellationToken);
}
