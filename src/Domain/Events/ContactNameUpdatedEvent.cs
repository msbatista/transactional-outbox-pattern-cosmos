using Domain.ValueObjects;

namespace Domain.Events;

public sealed class ContactNameUpdatedEvent : ContactDomainEvent
{
    public ContactNameUpdatedEvent(Guid contactId, Name contactName) : base(Guid.NewGuid(), contactId, nameof(ContactNameUpdatedEvent))
    {
        Name = contactName;
    }

    public Name Name { get; }
}
