namespace Domain.Events;

public abstract class ContactDomainEvent : Entity, IEvent
{
    protected ContactDomainEvent(Guid id, Guid contactId, string action)
    {
        Id = id;
        ContactId = contactId;
        Action = action;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid ContactId { get; }
    public string Action { get; }
    public DateTimeOffset CreatedAt { get; }
}
