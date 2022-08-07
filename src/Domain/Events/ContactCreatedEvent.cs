namespace Domain.Events;

public sealed class ContactCreatedEvent : ContactDomainEvent
{
    public ContactCreatedEvent(Guid contactId, Contact contact) : base(Guid.NewGuid(), contactId, nameof(ContactCreatedEvent))
    {
        Contact = contact;
    }

    public Contact Contact { get; }
}
