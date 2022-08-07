using MediatR;

namespace Domain.Events;

public interface IEvent : INotification
{
    Guid Id { get; }
    string Action { get; }
}
