namespace Domain.Events;

public sealed class ContactEmailUpdatedEvent : ContactDomainEvent
{
    public string Email { get; }

    public ContactEmailUpdatedEvent(Guid contactId, string email) : base(Guid.NewGuid(), contactId, nameof(ContactEmailUpdatedEvent))
    {
        Email = email;
    }
}
