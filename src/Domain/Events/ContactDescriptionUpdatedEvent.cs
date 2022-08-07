namespace Domain.Events;

public sealed class ContactDescriptionUpdatedEvent : ContactDomainEvent
{
    public ContactDescriptionUpdatedEvent(Guid contactId, string description) : base(Guid.NewGuid(), contactId, nameof(ContactDescriptionUpdatedEvent))
    {
        Description = description;
    }

    public string Description { get; }
}
