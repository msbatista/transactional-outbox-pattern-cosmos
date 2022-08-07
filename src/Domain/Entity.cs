namespace Domain;

public abstract class Entity : IEntity
{
    public Guid Id { get; protected init; }
}
