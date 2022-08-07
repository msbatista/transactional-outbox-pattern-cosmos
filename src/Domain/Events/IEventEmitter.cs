namespace Domain.Events;

public interface IEventEmitter<T> where T : IEvent
{
    void AddEvent(T domainEvent);
    void RemoveEvent(T domainEvent);
    void RemoveAllEvents();
    IReadOnlyList<T> DomainEvents { get; }
}
