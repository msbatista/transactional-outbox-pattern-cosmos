namespace Domain.Events;

public sealed class ContactDeletedEvent : ContactDomainEvent
{
    public ContactDeletedEvent(Guid contactId) : base(Guid.NewGuid(), contactId, nameof(ContactDeletedEvent))
    {
    }
}
