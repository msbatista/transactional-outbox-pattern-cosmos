namespace Domain.Events;

public interface IEventRepository
{
    void Create(ContactDomainEvent e);
}
