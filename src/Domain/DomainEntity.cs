using Domain.Events;

namespace Domain;

public class DomainEntity : Entity, IEventEmitter<IEvent>
{
    public DateTimeOffset CreatedAt { get; protected set; }
    public DateTimeOffset? ModifiedAt { get; protected set; }
    public DateTimeOffset? DeletedAt { get; protected set; }

    public bool Deleted { get; protected set; }
    protected bool IsNew { get; init; }

    private readonly List<IEvent> _events = new();

    public IReadOnlyList<IEvent> DomainEvents => _events.AsReadOnly();

    public virtual void AddEvent(IEvent domainEvent)
    {
        var i = _events.FindIndex(0, e => e.Action == domainEvent.Action);

        if (i < 0)
        {
            _events.Add(domainEvent);
        }
        else
        {
            _events.RemoveAt(i);
            _events.Insert(i, domainEvent);

        }
    }

    public void RemoveAllEvents()
    {
        _events.Clear();
    }

    public void RemoveEvent(IEvent domainEvent)
    {
        _events.Remove(domainEvent);
    }
}
